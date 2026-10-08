namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteResult
{
	public RouletteSpinResult Result { get; init; }
	public List<RouletteBetWithPayout> WinningBets { get; init; }
	public List<RouletteBet> LosingBets { get; init; }
}
