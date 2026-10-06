using DesmondBot.Features.Roulette.Application;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly RouletteMessageService _rouletteMessageService;

	public RouletteCommandModule(RouletteGameService rouletteGameService, RouletteMessageService rouletteMessageService)
	{
		_rouletteGameService = rouletteGameService;
		_rouletteMessageService = rouletteMessageService;
	}

	[SlashCommand("roulette", "Start a new game of roulette")]
	public async Task StartRoulette()
	{
		var game = _rouletteGameService.StartGame();
		var message = RouletteMessageBuilder.Create(game);
		await Context.Interaction.SendResponseAsync(InteractionCallback.Message(message));

		var response = await Context.Interaction.GetResponseAsync();
		var messageId = response.Id;
		var channelId = response.ChannelId;

		_rouletteMessageService.RegisterMessage(channelId, messageId);
	}
}
