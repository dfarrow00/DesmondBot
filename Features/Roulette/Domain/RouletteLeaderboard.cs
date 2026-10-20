namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteLeaderboard
{
	public required List<RouletteLeaderboardEntry> BiggestWin { get; init; }
	public required List<RouletteLeaderboardEntry> BiggestLoss { get; init; }
	public required List<RouletteLeaderboardEntry> LongestWinStreak { get; init; }
}
