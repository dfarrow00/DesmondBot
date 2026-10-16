namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteResult
{
	public RouletteSpinResult Result { get; init; }
	public List<RoulettePlayerResult> WinningBets { get; init; }
	public List<RoulettePlayerResult> LosingBets { get; init; }
}
