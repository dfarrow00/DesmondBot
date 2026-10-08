namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteBetWithPayout
{
	public RouletteBet Bet { get; init; }
	public decimal Payout { get; init; }
}
