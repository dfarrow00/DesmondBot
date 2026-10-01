using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public static class RouletteMessageBuilder
{
	public static InteractionMessageProperties Create(/*RouletteGame game*/)
	{
		var embed = new EmbedProperties
		{
			Title = "Roulette #"/* + game.Id*/,
			Description = "Betting is open!\n\nCloses in: 30 seconds",
			Color = new Color(0x2B2D31)
		};

		var components = new[]
		{
			new ActionRowProperties
			{
				new ButtonProperties(
					"roulette:red",
					"Red",
					ButtonStyle.Danger),

				new ButtonProperties(
					"roulette:black",
					"Black",
					ButtonStyle.Secondary),

				new ButtonProperties(
					"roulette:number",
					"Number",
					ButtonStyle.Primary)
			}
		};

		return new InteractionMessageProperties()
			.WithEmbeds(new List<EmbedProperties>{ embed })
			.WithComponents(components);
	}
}
