using DesmondBot.Features.Roulette.Application;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly RouletteGameService _rouletteGameService;
	private readonly IRouletteMessageService _rouletteMessageService;
	private readonly IRouletteGameRepository _rouletteGameRepository;
	private readonly ILogger<RouletteCommandModule> _logger;

	public RouletteCommandModule(RouletteGameService rouletteGameService,
		IRouletteMessageService rouletteMessageService,
		IRouletteGameRepository rouletteGameRepository,
		ILogger<RouletteCommandModule> logger)
	{
		_rouletteGameService = rouletteGameService;
		_rouletteMessageService = rouletteMessageService;
		_rouletteGameRepository = rouletteGameRepository;
		_logger = logger;
	}

	[SlashCommand("roulette", "Start a new game of roulette")]
	public async Task StartRoulette()
	{
		_logger.LogInformation("/roulette requested by {GlobalName}", Context.User.GlobalName);
		if (_rouletteGameService.HasActiveGame())
		{
			_logger.LogInformation("Roulette failed to start: A game is already running");
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
		_logger.LogInformation("Roulette game started successfully");
	}

	[SlashCommand("roulette-stats", "View roulette stats for a player")]
	public async Task<InteractionMessageProperties> GetRouletteStatsAsync(GuildUser player)
	{
		_logger.LogInformation("/roulette-stats requested by {GlobalName} for player {PlayerGlobalName}", Context.User.GlobalName, player.GlobalName);
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
