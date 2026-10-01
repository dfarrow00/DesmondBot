namespace DesmondBot.Features.Roulette.Domain;

public sealed record RouletteResult
{
	public int Number { get; init; }
	public RouletteColour Colour { get; init; }
}
