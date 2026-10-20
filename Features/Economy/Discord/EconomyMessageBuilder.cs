using DesmondBot.Features.Economy.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Economy.Discord;

public static class EconomyMessageBuilder
{
	public static EmbedProperties CreateMoneyLeaderboardEmbed(EconomyLeaderboard leaderboard)
	{
		return new EmbedProperties
		{
			Title = ":moneybag: Money Leaderboard",
			Color = new Color(0xFFD700),
			Fields =
			[
				new EmbedFieldProperties
				{
					Name = "Current Balance",
					Value = CreateLeaderboardEntries(leaderboard.CurrentBalance, value => $":money_with_wings: {value}"),
					Inline = false
				},
				new EmbedFieldProperties
				{
					Name = "Peak Balance",
					Value = CreateLeaderboardEntries(leaderboard.PeakBalance, value => $":moneybag: {value}"),
					Inline = false
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
