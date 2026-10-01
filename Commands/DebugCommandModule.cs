using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Commands;

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

