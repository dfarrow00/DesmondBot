using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Application;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteComponentModule : ComponentInteractionModule<ButtonInteractionContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly EconomyService _economyService;

	public RouletteComponentModule(RouletteGameService rouletteGameService, EconomyService economyService)
	{
		_rouletteGameService = rouletteGameService;
		_economyService = economyService;
	}

	[ComponentInteraction("roulette-bet")]
	public async Task OpenBetModalAsync()
	{
		var balance = await _economyService.GetBalanceAsync(Context.User.Id);
		await RespondAsync(InteractionCallback.Modal(RouletteModalBuilder.Create(balance)));
	}
}
