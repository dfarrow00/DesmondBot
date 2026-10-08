using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Discord;
using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public class RouletteGameService
{
	private RouletteGame? _activeGame;
	private readonly IEconomyRepository _economyRepository;
	private readonly RouletteMessageService _rouletteMessageService;
	private readonly Random _random = new();

	public RouletteGameService(IEconomyRepository economyRepository, RouletteMessageService rouletteMessageService)
	{
		_economyRepository = economyRepository;
		_rouletteMessageService = rouletteMessageService;
	}

	public RouletteGame StartGame()
	{
		_activeGame = new RouletteGame(1, DateTimeOffset.UtcNow.AddSeconds(32));
		ScheduleBettingClosed(_activeGame);

		return _activeGame;
	}

	public async Task<PlaceBetResult> PlaceBetAsync(ulong userId, int amount, RouletteBetType betType, int? number)
	{
		var bet = new RouletteBet
		{
			UserId = userId,
			Amount = amount,
			Type = betType,
			Number = number
		};

		var validationResult = await ValidateBetAsync(bet);
		if (validationResult != PlaceBetResult.Success)
		{
			return validationResult;
		}

		await _economyRepository.RemoveBalanceAsync(userId, amount);
		_activeGame!.Bets.Add(bet);

		return PlaceBetResult.Success;

		async Task<PlaceBetResult> ValidateBetAsync(RouletteBet bet)
		{
			if (_activeGame == null) return PlaceBetResult.NoActiveGame;

			if (DateTimeOffset.UtcNow >= _activeGame.BettingCloseTime) return PlaceBetResult.BettingClosed;

			if (_activeGame.Bets.Any(b => b.UserId == bet.UserId)) return PlaceBetResult.AlreadyBet;

			if (bet.Amount <= 0) return PlaceBetResult.InvalidAmount;

			if (bet.Type == RouletteBetType.Number)
			{
				if (bet.Number is null) return PlaceBetResult.NumberRequired;
				if (bet.Number < 0 || bet.Number > 36) return PlaceBetResult.InvalidNumber;
			}

			var balance = await _economyRepository.GetBalanceAsync(bet.UserId);
			if (bet.Amount > balance) return PlaceBetResult.InsufficientBalance;

			return PlaceBetResult.Success;
		}
	}

	public bool HasActiveGame()
	{
		return _activeGame != null;
	}

	public RouletteGame GetActiveGame()
	{
		return _activeGame ?? throw new InvalidOperationException("No active roulette game.");
	}

	private void ScheduleBettingClosed(RouletteGame game)
	{
		_ = Task.Run(async () =>
		{
			var delay = game.BettingCloseTime - DateTimeOffset.UtcNow;
			await Task.Delay(delay);
			await _rouletteMessageService.UpdateMessageBettingClosedAsync(game);
			ScheduleWheelSpin(game);
		});
	}

	private void ScheduleWheelSpin(RouletteGame game)
	{
		_ = Task.Run(async () =>
		{
			await Task.Delay(TimeSpan.FromSeconds(10));
			await SpinWheelAsync(game);
		});
	}

	private async Task SpinWheelAsync(RouletteGame game)
	{
		var spinResult = GenerateRouletteSpinResult();
		var winningBets = GetWinningBets(game.Bets, spinResult);
		var winningBetsWithPayouts = new List<RouletteBetWithPayout>();

		foreach (var bet in winningBets)
		{
			var payout = CalculatePayout(bet, spinResult);
			await _economyRepository.AddBalanceAsync(bet.UserId, (int)payout);
			winningBetsWithPayouts.Add(new RouletteBetWithPayout { Bet = bet, Payout = payout });
		}

		var losingBets = game.Bets.Where(b => !winningBets.Contains(b)).ToList();
		var result = new RouletteResult
		{
			Result = spinResult,
			WinningBets = winningBetsWithPayouts,
			LosingBets = losingBets
		};

		_activeGame = null;
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

	private List<RouletteBet> GetWinningBets(List<RouletteBet> bets, RouletteSpinResult result)
	{
		return bets.Where(bet => IsWinningBet(bet, result)).ToList();
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

	private decimal CalculatePayout(RouletteBet bet, RouletteSpinResult result)
	{
		return bet.Type switch
		{
			RouletteBetType.Red or RouletteBetType.Black => bet.Amount * 2,
			RouletteBetType.Number => bet.Amount * 36,
			_ => 0
		};
	}
}
