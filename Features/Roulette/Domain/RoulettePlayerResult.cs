namespace DesmondBot.Features.Roulette.Domain;

public sealed record RoulettePlayerResult
{
	public ulong UserId { get; init; }
	public int BetAmount { get; init; }
	public RouletteBetType BetType { get; init; }
	public int? BetNumber { get; init; }
	public bool IsWin { get; init; }
	public int Payout { get; init; }
	public int NetChange { get; init; }
}
