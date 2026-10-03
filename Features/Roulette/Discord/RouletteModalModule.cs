using DesmondBot.Features.Roulette.Application;
using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

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

		var amountText = Context.Components.OfType<Label>().Select(x => x.Component).OfType<TextInput>().First(x => x.CustomId == "roulette-bet-amount").Value
			?? throw new ArgumentNullException("Failed to place bet: Amount is null");

		var betTypeText = Context.Components.OfType<Label>().Select(x => x.Component).OfType<StringMenu>().First(x => x.CustomId == "roulette-bet-type").SelectedValues?.FirstOrDefault()
			?? throw new ArgumentNullException("Failed to place bet: Type is null");

		var numberText = Context.Components.OfType<Label>().Select(x => x.Component).OfType<TextInput>().First(x => x.CustomId == "roulette-bet-number").Value
			?? throw new ArgumentNullException("Failed to place bet: Number is null"); ;

		if (int.TryParse(amountText, out int amount) == false)
		{
			await RespondAsync(InteractionCallback.Message("Failed to place bet: Amount is not a valid integer"));
			return;
		}

		if (Enum.TryParse<RouletteBetType>(betTypeText, out var betType) == false)
		{
			await RespondAsync(InteractionCallback.Message("Failed to place bet: Bet type is not valid"));
			return;
		}

		if (betType == RouletteBetType.Number && numberText == "")
		{
			await RespondAsync(InteractionCallback.Message("Failed to place bet: Number is not provided"));
			return;
		}

		if (int.TryParse(numberText, out var number) == false && betType == RouletteBetType.Number)
		{
			await RespondAsync(InteractionCallback.Message("Failed to place bet: Number is not a valid integer"));
			return;
		}

		await _rouletteGameService.PlaceBetAsync(userId, amount, betType, number);
	}
}
