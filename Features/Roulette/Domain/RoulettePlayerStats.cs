namespace DesmondBot.Features.Roulette.Domain;

public sealed record RoulettePlayerStats
{
	public int TotalGamesPlayed { get; init; }
	public decimal WinRate { get; init; }
	public int TotalWagered { get; init; }
	public int PeakBalance { get; init; }
	public int BiggestWin { get; init; }
	public int BiggestLoss { get; init; }
	public int LongestWinningStreak { get; init; }
	public int CurrentWinningStreak { get; init; }
}
