namespace DesmondBot.Features.Roulette.Domain;

public sealed class RouletteGame
{
	public int Id { get; }
	public DateTimeOffset BettingCloseTime { get; }
	public List<RouletteBet> Bets { get; } = new();

	public RouletteGame(int id, DateTimeOffset bettingCloseTime)
	{
		Id = id;
		BettingCloseTime = bettingCloseTime;
	}
}
