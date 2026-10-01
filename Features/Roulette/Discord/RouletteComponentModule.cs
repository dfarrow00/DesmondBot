using NetCord.Services.ComponentInteractions;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteComponentModule : ComponentInteractionModule<ButtonInteractionContext>
{
	[ComponentInteraction("roulette:red")]
	public async Task RedAsync()
	{
		
	}

	[ComponentInteraction("roulette:black")]
	public async Task BlackAsync()
	{

	}

	[ComponentInteraction("roulette:number")]
	public async Task NumberAsync()
	{

	}
}
