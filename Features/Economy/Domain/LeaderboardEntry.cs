namespace DesmondBot.Features.Economy.Domain;

public sealed record LeaderboardEntry
{
	public ulong UserId { get; init; }
	public int Balance { get; init; }
}
