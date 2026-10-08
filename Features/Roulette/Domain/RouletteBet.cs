namespace DesmondBot.Features.Roulette.Domain;

public sealed class RouletteBet
{
	public ulong UserId { get; init; }
	public int Amount { get; init; }
	public RouletteBetType Type { get; init; }
	public int? Number { get; init; }
}
