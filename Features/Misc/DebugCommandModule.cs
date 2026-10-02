using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Misc;

public class DebugCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	[SlashCommand("ping", "Check if DesmondBot is responsive")]
	public static InteractionMessageProperties Ping()
	{
		return new InteractionMessageProperties
		{
			Content = "Pong!",
			Flags = MessageFlags.Ephemeral
		};
	}
}

