using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public class RouletteGameService
{
	private RouletteGame? _activeGame;

	public async Task<RouletteGame> StartGameAsync()
	{
		if (_activeGame != null)
		{
			throw new InvalidOperationException("Cannot start roulette, another game is already running");
		}

		_activeGame = new RouletteGame(1, DateTime.Now.AddSeconds(32));

		return _activeGame;
	}

	public async Task PlaceBetAsync(ulong userId, int amount, RouletteBetType betType, int? number)
	{
		// Get user balance
		//Check user has enough funds for bet
		//Create and place bet
		//Notify user
	}
}
