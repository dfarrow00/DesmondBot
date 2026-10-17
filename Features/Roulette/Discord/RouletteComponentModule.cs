using DesmondBot.Features.Economy.Application;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteComponentModule : ComponentInteractionModule<ButtonInteractionContext>
{
	private readonly IEconomyRepository _economyRepository;

	public RouletteComponentModule(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	[ComponentInteraction("roulette-bet")]
	public async Task OpenBetModalAsync()
	{
		var balance = await _economyRepository.GetBalanceAsync(Context.User.Id);
		await RespondAsync(InteractionCallback.Modal(RouletteModalBuilder.Create(balance)));
	}
}
