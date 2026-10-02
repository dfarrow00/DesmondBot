using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public static class RouletteMessageBuilder
{
	public static InteractionMessageProperties Create(RouletteGame game)
	{
			var embed = new EmbedProperties
			{
				Title = $":slot_machine: Roulette #{game.Id}",
				Description = $"Place your bets!\nThe wheel will spin **<t:{game.BettingCloseTime.ToUnixTimeSeconds()}:R>**",
				Color = new Color(0xFF3B3B),
				Fields = new List<EmbedFieldProperties>
				{
					new EmbedFieldProperties { Name = "Min Bet", Value = ":moneybag: 1", Inline = true },
					new EmbedFieldProperties { Name = "Max Bet", Value = ":moneybag: 100,000", Inline = true },
					new EmbedFieldProperties { Name = "Pot (so far)", Value = $":busts_in_silhouette: {game.Bets.Sum(b => b.Amount)}", Inline = true },
					new EmbedFieldProperties
					{
						Name = $"Current Bets ({game.Bets.Count})",
						Value = GetPlacedBetsString(game.Bets),
						Inline = false
					}
				},
				Footer = new EmbedFooterProperties
				{
					Text = "Place your bet before the timer runs out!",
				}
			};

			var components = new[]
			{
				new ActionRowProperties
				{
					new ButtonProperties("roulette-bet", "Place Bet", ButtonStyle.Primary)
				}
			};

			return new InteractionMessageProperties()
				.WithEmbeds(new List<EmbedProperties> { embed })
				.WithComponents(components);
	}

	private static string GetPlacedBetsString(List<RouletteBet> bets)
	{
		var output = "";
		foreach (var bet in bets)
		{
			string betTypeString;
			switch (bet.Type)
			{
				case RouletteBetType.Red:
					betTypeString = ":red_circle: Red";
					break;
				case RouletteBetType.Black:
					betTypeString = ":black_circle: Black";
					break;
				case RouletteBetType.Number:
					betTypeString = $":1234: {bet.Number}";
					break;
				default:
					throw new Exception($"Invalid roulette bet type {(int)bet.Type}");
			}

			output += $"• <@{bet.UserId}> - :moneybag: {bet.Amount} - {betTypeString}\n";
		}

		return output;
	}
}
