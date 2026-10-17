using DesmondBot.Features.Roulette.Application;
using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteModalModule : ComponentInteractionModule<ModalInteractionContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly IRouletteMessageService _rouletteMessageService;

	public RouletteModalModule(RouletteGameService rouletteGameService, IRouletteMessageService rouletteMessageService)
	{
		_rouletteGameService = rouletteGameService;
		_rouletteMessageService = rouletteMessageService;
	}

	[ComponentInteraction("roulette-bet-submit")]
	public async Task SubmitBetAsync()
	{
		var userId = Context.User.Id;

		var amountText = Context.Components
			.OfType<Label>()
			.Select(x => x.Component)
			.OfType<TextInput>()
			.FirstOrDefault(x => x.CustomId == "roulette-bet-amount")
			?.Value;

		if (string.IsNullOrWhiteSpace(amountText) || int.TryParse(amountText, out var amount) == false)
		{
			await RespondEphemeralAsync(":exclamation: Amount is not a valid integer");
			return;
		}

		var betTypeText = Context.Components
			.OfType<Label>()
			.Select(x => x.Component)
			.OfType<StringMenu>()
			.FirstOrDefault(x => x.CustomId == "roulette-bet-type")
			?.SelectedValues?
			.FirstOrDefault();

		if (Enum.TryParse<RouletteBetType>(betTypeText, out var betType) == false)
		{
			await RespondEphemeralAsync(":exclamation: Bet type is not valid");
			return;
		}

		var numberText = Context.Components
			.OfType<Label>()
			.Select(x => x.Component)
			.OfType<TextInput>()
			.FirstOrDefault(x => x.CustomId == "roulette-bet-number")
			?.Value;

		int? number = null;
		if (string.IsNullOrWhiteSpace(numberText) == false)
		{
			if (int.TryParse(numberText, out var parsedNumber) == false)
			{
				await RespondEphemeralAsync(":exclamation: Number is not a valid integer");
				return;
			}

			number = parsedNumber;
		}

		var result = await _rouletteGameService.PlaceBetAsync(userId, amount, betType, number);

		if (result == PlaceBetResult.Success)
		{
			await Context.Interaction.SendResponseAsync(InteractionCallback.DeferredModifyMessage);
			var game = _rouletteGameService.GetActiveGame();
			await _rouletteMessageService.UpdateStartMessageAsync(game);
			return;
		}

		await RespondEphemeralAsync(GetResultMessage(result));
	}

	private Task RespondEphemeralAsync(string message)
	{
		return RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
		{
			Content = message,
			Flags = MessageFlags.Ephemeral
		}));
	}

	private static string GetResultMessage(PlaceBetResult result)
	{
		return result switch
		{
			PlaceBetResult.InsufficientBalance => ":exclamation: Insufficient funds",
			PlaceBetResult.BettingClosed => ":exclamation: Betting is closed",
			PlaceBetResult.NoActiveGame => ":exclamation: No active roulette game",
			PlaceBetResult.AlreadyBet => ":exclamation: You've already placed a bet for this game",
			PlaceBetResult.InvalidAmount => ":exclamation: Amount must be greater than zero",
			PlaceBetResult.NumberRequired => ":exclamation: Number bet type requires a number",
			PlaceBetResult.InvalidNumber => ":exclamation: Number must be between 0 and 36",
			_ => ":exclamation: Failed to place bet"
		};
	}
}
