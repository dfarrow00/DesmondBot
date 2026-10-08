namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteSpinResult
{
	public int Number { get; init; }
	public RouletteColour Colour { get; init; }
}
