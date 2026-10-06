using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Discord;
using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public class RouletteGameService
{
	private RouletteGame? _activeGame;
	private readonly EconomyService _economyService;
	private readonly RouletteMessageService _rouletteMessageService;

	public RouletteGameService(EconomyService economyService, RouletteMessageService rouletteMessageService)
	{
		_economyService = economyService;
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

		await _economyService.RemoveBalanceAsync(userId, amount);
		_activeGame!.Bets.Add(bet);

		return PlaceBetResult.Success;
	}

	private async Task<PlaceBetResult> ValidateBetAsync(RouletteBet bet)
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

		var balance = await _economyService.GetBalanceAsync(bet.UserId);
		if (bet.Amount > balance) return PlaceBetResult.InsufficientBalance;

		return PlaceBetResult.Success;
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
		});
	}
}
