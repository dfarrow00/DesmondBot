using DesmondBot.Features.Roulette.Application;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly RouletteGameService _rouletteGameService;

	public RouletteCommandModule(RouletteGameService rouletteGameService)
	{
		_rouletteGameService = rouletteGameService;
	}

	[SlashCommand("roulette", "Start a new game of roulette")]
	public async Task StartRoulette()
	{
		//Create game using service, send game to message builder
		var message = RouletteMessageBuilder.Create();
		await Context.Interaction.SendResponseAsync(InteractionCallback.Message(message));
	}
}
