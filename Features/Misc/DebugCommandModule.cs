using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Misc;

public class DebugCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly ILogger<DebugCommandModule> _logger;

	public DebugCommandModule(ILogger<DebugCommandModule> logger)
	{
		_logger = logger;
	}

	[SlashCommand("ping", "Check if DesmondBot is responsive")]
	public InteractionMessageProperties Ping()
	{
		_logger.LogInformation("/ping command used by {GlobalName}", Context.User.GlobalName);
		return new InteractionMessageProperties
		{
			Content = "Pong!",
			Flags = MessageFlags.Ephemeral
		};
	}
}

