namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteLeaderboardEntry
{
	public ulong UserId { get; init; }
	public int Value { get; init; }
}
