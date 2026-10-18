using DesmondBot.Features.Economy.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Economy.Discord;

public static class EconomyMessageBuilder
{
	public static EmbedProperties CreateLeaderboardEmbed(EconomyLeaderboard leaderboard)
	{
		return new EmbedProperties
		{
			Title = ":trophy: Leaderboard",
			Color = new Color(0xFFD700),
			Fields =
			[
				new EmbedFieldProperties
				{
					Name = "Current Balance",
					Value = CreateLeaderboardEntries(leaderboard.CurrentBalance, value => $":money_with_wings: {value}"),
					Inline = true
				},
				new EmbedFieldProperties
				{
					Name = "Peak Balance",
					Value = CreateLeaderboardEntries(leaderboard.PeakBalance, value => $":moneybag: {value}"),
					Inline = true
				},
				new EmbedFieldProperties
				{
					Name = "Longest Win Streak",
					Value = CreateLeaderboardEntries(leaderboard.LongestWinStreak, value => $":fire: {value.ToString()}"),
					Inline = true
				},
				new EmbedFieldProperties
				{
					Name = "Biggest Win",
					Value = CreateLeaderboardEntries(leaderboard.BiggestWin, value => $":money_with_wings: +{value}"),
					Inline = true
				}
			]
		};
	}

	private static string CreateLeaderboardEntries(IReadOnlyList<LeaderboardEntry> entries, Func<int, string> formatValue)
	{
		return entries.Count == 0
			? "No balances found."
			: string.Join("\n", entries.Select((entry, index) =>
			{
				var position = index switch
				{
					0 => ":first_place:",
					1 => ":second_place:",
					2 => ":third_place:",
					_ => $"#{index + 1}"
				};

				return $"{position} <@{entry.UserId}> — {formatValue(entry.Value)}";
			}));
	}
}
