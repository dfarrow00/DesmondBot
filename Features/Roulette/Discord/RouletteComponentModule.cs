using DesmondBot.Features.Roulette.Application;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteComponentModule : ComponentInteractionModule<ButtonInteractionContext>
{
	private readonly RouletteGameService _rouletteGameService;

	public RouletteComponentModule(RouletteGameService rouletteGameService)
	{
		_rouletteGameService = rouletteGameService;
	}

	[ComponentInteraction("roulette-bet")]
	public async Task OpenBetModalAsync()
	{
		await RespondAsync(InteractionCallback.Modal(RouletteModalBuilder.Create()));
	}
}
