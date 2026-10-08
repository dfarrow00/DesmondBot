using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Roulette.Application;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteComponentModule : ComponentInteractionModule<ButtonInteractionContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly IEconomyRepository _economyRepository;

	public RouletteComponentModule(RouletteGameService rouletteGameService, IEconomyRepository economyRepository)
	{
		_rouletteGameService = rouletteGameService;
		_economyRepository = economyRepository;
	}

	[ComponentInteraction("roulette-bet")]
	public async Task OpenBetModalAsync()
	{
		var balance = await _economyRepository.GetBalanceAsync(Context.User.Id);
		await RespondAsync(InteractionCallback.Modal(RouletteModalBuilder.Create(balance)));
	}
}
