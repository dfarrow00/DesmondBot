using DesmondBot.Features.Roulette.Application;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly RouletteMessageService _rouletteMessageService;
	private readonly IRouletteGameRepository _rouletteGameRepository;

	public RouletteCommandModule(RouletteGameService rouletteGameService,
		RouletteMessageService rouletteMessageService,
		IRouletteGameRepository rouletteGameRepository)
	{
		_rouletteGameService = rouletteGameService;
		_rouletteMessageService = rouletteMessageService;
		_rouletteGameRepository = rouletteGameRepository;
	}

	[SlashCommand("roulette", "Start a new game of roulette")]
	public async Task StartRoulette()
	{
		if (_rouletteGameService.HasActiveGame())
		{
			await RespondAsync(InteractionCallback.Message(new InteractionMessageProperties
			{
				Content = ":exclamation: A roulette game is already running",
				Flags = MessageFlags.Ephemeral
			}));
			return;
		}

		var game = await _rouletteGameService.StartGameAsync();
		var message = RouletteMessageBuilder.Create(game);
		await Context.Interaction.SendResponseAsync(InteractionCallback.Message(message));

		var response = await Context.Interaction.GetResponseAsync();
		var messageId = response.Id;
		var channelId = response.ChannelId;

		_rouletteMessageService.RegisterMessage(channelId, messageId);
	}

	[SlashCommand("stats", "View roulette stats for a player")]
	public async Task<InteractionMessageProperties> GetRouletteStatsAsync(GuildUser player)
	{
		if (player.IsBot)
		{
			return new InteractionMessageProperties
			{
				Content = ":exclamation: DesmondBot does not have roulette stats",
				Flags = MessageFlags.Ephemeral
			};
		}

		var stats = await _rouletteGameRepository.GetPlayerStatsAsync(player.Id);
		var embed = RouletteMessageBuilder.CreateStatsEmbed(player.Id, stats);

		return new InteractionMessageProperties().WithEmbeds(new[] { embed });
	}
}
