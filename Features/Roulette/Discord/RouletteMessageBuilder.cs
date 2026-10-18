using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public static class RouletteMessageBuilder
{
	public static InteractionMessageProperties Create(RouletteGame game)
	{
		var embed = CreateStartEmbed(game);

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

	public static EmbedProperties CreateStartEmbed(RouletteGame game)
	{
		return new EmbedProperties
		{
			Title = $":slot_machine: Roulette #{game.Id}",
			Description = $"Place your bets!\nThe wheel will spin **<t:{game.BettingCloseTime.ToUnixTimeSeconds()}:R>**",
			Color = new Color(0x176B45),
			Fields = new List<EmbedFieldProperties>
			{
				new EmbedFieldProperties { Name = "Min Bet", Value = ":money_with_wings: 1", Inline = true },
				new EmbedFieldProperties { Name = "Max Bet", Value = ":money_with_wings: 100k", Inline = true },
				new EmbedFieldProperties { Name = "Pot", Value = $":moneybag: {game.Bets.Sum(b => b.Amount)}", Inline = true },
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
	}

	public static EmbedProperties CreateBettingClosedEmbed(RouletteGame game)
	{
		return new EmbedProperties
		{
			Title = $":slot_machine: Roulette #{game.Id}",
			Description = ":no_entry_sign: **Bets are now closed** :no_entry_sign:\n\nThe wheel is spinning...",
			Color = new Color(0x176B45),
			Fields = new List<EmbedFieldProperties>
			{
				new EmbedFieldProperties { Name = "Pot", Value = $":moneybag: {game.Bets.Sum(b => b.Amount)}", Inline = true },
				new EmbedFieldProperties
				{
					Name = $"Final Bets ({game.Bets.Count})",
					Value = GetPlacedBetsString(game.Bets),
					Inline = false
				}
			},
			Footer = new EmbedFooterProperties
			{
				Text = "No more bets can be placed.",
			}
		};
	}

	public static EmbedProperties CreateResultEmbed(RouletteGame game, RouletteResult rouletteResult)
	{
		var result = rouletteResult.Result;
		var colourEmoji = result.Colour switch
		{
			RouletteColour.Red => ":red_circle:",
			RouletteColour.Black => ":black_circle:",
			RouletteColour.Green => ":green_circle:",
			_ => throw new Exception($"Invalid roulette colour {(int)result.Colour}"),
		};

		var betResultFields = new List<EmbedFieldProperties>();
		if (rouletteResult.WinningBets.Count > 0)
		{
			betResultFields.Add(new EmbedFieldProperties
			{
				Name = $":trophy: Winners ({rouletteResult.WinningBets.Count})",
				Value = GetWinningBetsString(rouletteResult.WinningBets),
				Inline = false
			});
		}

		if (rouletteResult.LosingBets.Count > 0)
		{
			betResultFields.Add(new EmbedFieldProperties
			{
				Name = $":skull: Losers ({rouletteResult.LosingBets.Count})",
				Value = GetLosingBetsString(rouletteResult.LosingBets),
				Inline = false
			});
		}

		return new EmbedProperties
		{
			Title = $":slot_machine: Roulette #{game.Id}",
			Description = $"The wheel has landed on: \u2003**{colourEmoji} {result.Number}**",
			Color = new Color(0x176B45),
			Fields = betResultFields
		};
	}

	public static EmbedProperties CreateStatsEmbed(ulong playerId, RoulettePlayerStats stats)
	{
		return new EmbedProperties
		{
			Title = ":slot_machine: Roulette Stats",
			Description = $"<@{playerId}>",
			Color = new Color(0x6554C0),
			Fields = new List<EmbedFieldProperties>
			{
				new() { Name = "Games Played", Value = $":game_die: {stats.TotalGamesPlayed.ToString()}", Inline = true },
				new() { Name = "Win Rate", Value = $"{stats.WinRate:0.##}%", Inline = true },
				new() { Name = "Total Wagered", Value = $":money_with_wings: {stats.TotalWagered}", Inline = true },
				new() { Name = "Peak Balance", Value = $":moneybag: {stats.PeakBalance}", Inline = true },
				new() { Name = "Biggest Win", Value = $":money_with_wings: +{stats.BiggestWin}", Inline = true },
				new() { Name = "Biggest Loss", Value = $":money_with_wings: {stats.BiggestLoss}", Inline = true },
				new() { Name = "Current Win Streak", Value = $":fire: {stats.CurrentWinningStreak}", Inline = true },
				new() { Name = "Longest Win Streak", Value = $":fire: {stats.LongestWinningStreak}", Inline = true }
			}
		};
	}

	private static string GetPlacedBetsString(List<RouletteBet> bets)
	{
		return bets.Count == 0
			? "None"
			: string.Join("", bets.Select(FormatPlacedBetLine));
	}

	private static string GetWinningBetsString(List<RoulettePlayerResult> bets)
	{
		return bets.Count == 0
			? "None"
			: string.Join("", bets.Select(FormatWinningBetLine));
	}

	private static string GetLosingBetsString(List<RoulettePlayerResult> bets)
	{
		return bets.Count == 0
			? "None"
			: string.Join("", bets.Select(FormatLosingBetLine));
	}

	private static string FormatBetType(RouletteBetType betType, int? number)
	{
		return betType switch
		{
			RouletteBetType.Red => ":red_circle: Red",
			RouletteBetType.Black => ":black_circle: Black",
			RouletteBetType.Number => $":dart: {number}",
			_ => throw new Exception($"Invalid roulette bet type {(int)betType}"),
		};
	}

	private static string FormatPlacedBetLine(RouletteBet bet)
	{
		return $"<@{bet.UserId}>\u2003-\u2003{FormatBetType(bet.Type, bet.Number)}\u2003-\u2003:money_with_wings: {bet.Amount}\n";
	}

	private static string FormatWinningBetLine(RoulettePlayerResult playerResult)
	{
		return $"<@{playerResult.UserId}>\u2003-\u2003{FormatBetType(playerResult.BetType, playerResult.BetNumber)}\u2003-\u2003:money_with_wings: +{playerResult.Payout}\n";
	}

	private static string FormatLosingBetLine(RoulettePlayerResult playerResult)
	{
		return $"<@{playerResult.UserId}>\u2003-\u2003{FormatBetType(playerResult.BetType, playerResult.BetNumber)}\u2003-\u2003:money_with_wings: -{playerResult.BetAmount}\n";
	}
}
