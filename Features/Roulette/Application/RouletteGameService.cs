using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Discord;
using DesmondBot.Features.Roulette.Domain;
using NetCord.Gateway;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Application;

public class RouletteGameService
{
	private RouletteGame? _activeGame;
	private readonly EconomyService _economyService;
	private ulong? _channelId;
	private ulong? _messageId;
	private readonly RestClient _restClient;

	public RouletteGameService(EconomyService economyService, RestClient restClient)
	{
		_economyService = economyService;
		_restClient = restClient;
	}

	public RouletteGame StartGame()
	{
		if (_activeGame != null)
		{
			throw new InvalidOperationException("Cannot start roulette, another game is already running");
		}

		_activeGame = new RouletteGame(1, DateTimeOffset.UtcNow.AddSeconds(32));

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

		if (DateTimeOffset.UtcNow > _activeGame.BettingCloseTime) return PlaceBetResult.BettingClosed;

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

	public void RegisterMessage(ulong channelId, ulong messageId)
	{
		_channelId = channelId;
		_messageId = messageId;
	}

	//TODO maybe move this into a message service or something
	public async Task UpdateMessageAsync()
	{
		if (_activeGame is null || _channelId is null || _messageId is null)
		{
			throw new InvalidOperationException("No active roulette message is registered.");
		}

		await _restClient.ModifyMessageAsync(
			_channelId.Value,
			_messageId.Value,
			options =>
			{
				options.Embeds =
				[
					RouletteMessageBuilder.CreateEmbed(_activeGame)
				];
			});
	}
}
