namespace DesmondBot.Features.Economy.Domain;

public sealed record EconomyLeaderboard
{
	public required List<LeaderboardEntry> CurrentBalance { get; init; }
	public required List<LeaderboardEntry> PeakBalance { get; init; }
}
