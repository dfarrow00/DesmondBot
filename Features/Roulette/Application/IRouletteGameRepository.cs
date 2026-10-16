using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public interface IRouletteGameRepository
{
	Task<int> GetNextGameIdAsync();
	Task SaveGameOutcomeAsync(RouletteSpinResult spinResult,
		List<RoulettePlayerResult> playerOutcomes,
		DateTimeOffset completedAtUtc);
}
