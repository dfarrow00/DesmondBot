using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Discord;
using DesmondBot.Features.Roulette.Domain;
using Microsoft.Extensions.Logging;

namespace DesmondBot.Features.Roulette.Application;

public class RouletteGameService
{
	private RouletteGame? _activeGame;
	private readonly IEconomyRepository _economyRepository;
	private readonly IRouletteGameRepository _rouletteGameRepository;
	private readonly RouletteMessageService _rouletteMessageService;
	private readonly ILogger<RouletteGameService> _logger;
	private readonly Random _random = new();
	private readonly SemaphoreSlim _rouletteBetLock = new(1, 1);
	private CancellationTokenSource? _gameCts;

	public RouletteGameService(IEconomyRepository economyRepository,
		IRouletteGameRepository rouletteGameRepository,
		RouletteMessageService rouletteMessageService,
		ILogger<RouletteGameService> logger)
	{
		_economyRepository = economyRepository;
		_rouletteGameRepository = rouletteGameRepository;
		_rouletteMessageService = rouletteMessageService;
		_logger = logger;
	}

	public async Task<RouletteGame> StartGameAsync()
	{
		RouletteGame game;

		await _rouletteBetLock.WaitAsync();
		try
		{
			var gameId = await _rouletteGameRepository.GetNextGameIdAsync();
			game = new RouletteGame(gameId, DateTimeOffset.UtcNow.AddSeconds(32));
			_activeGame = game;
		}
		finally
		{
			_rouletteBetLock.Release();
		}

		_gameCts?.Cancel();
		_gameCts?.Dispose();
		_gameCts = new CancellationTokenSource();

		_logger.LogInformation("Roulette game started. Betting closes at {BettingCloseTimeUtc}.", game.BettingCloseTime);

		_ = ScheduleBettingClosedAsync(game, _gameCts.Token);

		return game;
	}

	public async Task<PlaceBetResult> PlaceBetAsync(ulong userId, int amount, RouletteBetType betType, int? number)
	{
		await _rouletteBetLock.WaitAsync();
		try
		{
			if (_activeGame == null)
			{
				return PlaceBetResult.NoActiveGame;
			}

			var bet = new RouletteBet
			{
				UserId = userId,
				Amount = amount,
				Type = betType,
				Number = number
			};

			var validationResult = ValidateBet(bet);
			if (validationResult != PlaceBetResult.Success)
			{
				return validationResult;
			}

			var debited = await _economyRepository.TryRemoveBalanceAsync(userId, amount);
			if (!debited)
			{
				return PlaceBetResult.InsufficientBalance;
			}

			_activeGame.Bets.Add(bet);
			return PlaceBetResult.Success;
		}
		finally
		{
			_rouletteBetLock.Release();
		}
	}

	private PlaceBetResult ValidateBet(RouletteBet validatedBet)
	{
		if (_activeGame == null) return PlaceBetResult.NoActiveGame;

		if (DateTimeOffset.UtcNow >= _activeGame.BettingCloseTime) return PlaceBetResult.BettingClosed;

		if (_activeGame.Bets.Any(b => b.UserId == validatedBet.UserId)) return PlaceBetResult.AlreadyBet;

		if (validatedBet.Amount <= 0) return PlaceBetResult.InvalidAmount;

		if (validatedBet.Type == RouletteBetType.Number)
		{
			if (validatedBet.Number is null) return PlaceBetResult.NumberRequired;
			if (validatedBet.Number < 0 || validatedBet.Number > 36) return PlaceBetResult.InvalidNumber;
		}

		return PlaceBetResult.Success;
	}

	public bool HasActiveGame()
	{
		_rouletteBetLock.Wait();
		try
		{
			return _activeGame != null;
		}
		finally
		{
			_rouletteBetLock.Release();
		}
	}

	public RouletteGame GetActiveGame()
	{
		_rouletteBetLock.Wait();
		try
		{
			return _activeGame ?? throw new InvalidOperationException("No active roulette game.");
		}
		finally
		{
			_rouletteBetLock.Release();
		}
	}

	private async Task ScheduleBettingClosedAsync(RouletteGame game, CancellationToken token)
	{
		var delay = game.BettingCloseTime - DateTimeOffset.UtcNow;
		await Task.Delay(delay, token);

		_logger.LogInformation("Roulette betting closed with {TotalBets} bet(s).", game.Bets.Count);
		await _rouletteMessageService.UpdateMessageBettingClosedAsync(game);
		_ = ScheduleWheelSpinAsync(game, token);
	}

	private async Task ScheduleWheelSpinAsync(RouletteGame game, CancellationToken token)
	{
		await Task.Delay(TimeSpan.FromSeconds(10), token);
		_ = SpinWheelAsync(game);
	}

	private async Task SpinWheelAsync(RouletteGame game)
	{
		var spinResult = GenerateRouletteSpinResult();
		var playerResults = GetPlayerResults(game.Bets, spinResult);

		var result = new RouletteResult
		{
			Result = spinResult,
			WinningBets = playerResults.Where(r => r.IsWin).ToList(),
			LosingBets = playerResults.Where(r => r.IsWin == false).ToList()
		};

		await _rouletteGameRepository.SaveGameOutcomeAsync(spinResult, playerResults, DateTimeOffset.UtcNow);

		_logger.LogInformation("Roulette spun {Number} ({Colour}).",spinResult.Number,spinResult.Colour);

		await _rouletteBetLock.WaitAsync();
		try
		{
			_activeGame = null;
		}
		finally
		{
			_rouletteBetLock.Release();
		}

		await _rouletteMessageService.UpdateMessageResultAsync(game, result);
		_rouletteMessageService.ResetMessage();
	}

	private RouletteSpinResult GenerateRouletteSpinResult()
	{
		var number = _random.Next(0, 37);
		var colour = number switch
		{
			0 => RouletteColour.Green,
			_ => IsRed(number) ? RouletteColour.Red : RouletteColour.Black
		};

		return new RouletteSpinResult { Number = number, Colour = colour };
	}

	private bool IsRed(int number)
	{
		var redNumbers = new[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
		return redNumbers.Contains(number);
	}

	private List<RoulettePlayerResult> GetPlayerResults(List<RouletteBet> bets, RouletteSpinResult result)
	{
		return bets.Select(bet =>
		{
			var isWin = IsWinningBet(bet, result);
			var payout = isWin ? CalculatePayout(bet) : 0;

			return new RoulettePlayerResult
			{
				UserId = bet.UserId,
				BetAmount = bet.Amount,
				BetType = bet.Type,
				BetNumber = bet.Number,
				IsWin = isWin,
				Payout = payout,
				NetChange = isWin ? payout - bet.Amount : -bet.Amount
			};
		}).ToList();
	}

	private bool IsWinningBet(RouletteBet bet, RouletteSpinResult result)
	{
		return bet.Type switch
		{
			RouletteBetType.Red => result.Colour == RouletteColour.Red,
			RouletteBetType.Black => result.Colour == RouletteColour.Black,
			RouletteBetType.Number => result.Number == bet.Number,
			_ => false
		};
	}

	private int CalculatePayout(RouletteBet bet)
	{
		return bet.Type switch
		{
			RouletteBetType.Red or RouletteBetType.Black => bet.Amount * 2,
			RouletteBetType.Number => bet.Amount * 36,
			_ => 0
		};
	}
}
