using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public interface IRouletteGameRepository
{
	Task<int> GetNextGameIdAsync();
	Task<RouletteLeaderboard> GetLeaderboardAsync();
	Task<RoulettePlayerStats> GetPlayerStatsAsync(ulong userId);
	Task SaveGameOutcomeAsync(RouletteSpinResult spinResult,
		List<RoulettePlayerResult> playerOutcomes,
		DateTimeOffset completedAtUtc);
}
