using DesmondBot.Features.Roulette.Application;
using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;
using System.Threading.Channels;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteModalModule : ComponentInteractionModule<ModalInteractionContext>
{
	private readonly RouletteGameService _rouletteGameService;

	public RouletteModalModule(RouletteGameService rouletteGameService)
	{
		_rouletteGameService = rouletteGameService;
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
			await RespondEphemeralAsync("Failed to place bet: Amount is not a valid integer");
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
			await RespondEphemeralAsync("Failed to place bet: Bet type is not valid");
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
				await RespondEphemeralAsync("Failed to place bet: Number is not a valid integer");
				return;
			}

			number = parsedNumber;
		}

		var result = await _rouletteGameService.PlaceBetAsync(userId, amount, betType, number);
		await RespondEphemeralAsync(GetResultMessage(result));

		if (result == PlaceBetResult.Success)
		{
			_rouletteGameService.UpdateMessageAsync();
		}
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
			PlaceBetResult.Success => "Bet placed successfully",
			PlaceBetResult.InsufficientBalance => "Failed to place bet: Insufficient balance",
			PlaceBetResult.BettingClosed => "Failed to place bet: Betting is closed",
			PlaceBetResult.NoActiveGame => "Failed to place bet: No active roulette game",
			PlaceBetResult.AlreadyBet => "Failed to place bet: You already placed a bet for this game",
			PlaceBetResult.InvalidAmount => "Failed to place bet: Amount must be greater than zero",
			PlaceBetResult.NumberRequired => "Failed to place bet: Number bet requires a number",
			PlaceBetResult.InvalidNumber => "Failed to place bet: Number must be between 0 and 36",
			_ => "Failed to place bet"
		};
	}
}
