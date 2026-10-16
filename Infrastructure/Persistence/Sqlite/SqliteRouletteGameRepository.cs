using DesmondBot.Features.Roulette.Application;
using DesmondBot.Features.Roulette.Domain;
using Microsoft.Data.Sqlite;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteRouletteGameRepository : IRouletteGameRepository
{
	private readonly SqliteConnectionFactory _connectionFactory;

	public SqliteRouletteGameRepository(SqliteConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task<int> GetNextGameIdAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT COALESCE(MAX(Id), 0) + 1
			FROM RouletteGames;
			";

		var result = await command.ExecuteScalarAsync();
		return Convert.ToInt32(result);
	}

	public async Task SaveGameOutcomeAsync(RouletteSpinResult spinResult,
		List<RoulettePlayerResult> playerResults,
		DateTimeOffset completedAtUtc)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var transaction = connection.BeginTransaction();

		await using var gameCommand = connection.CreateCommand();
		gameCommand.Transaction = transaction;
		gameCommand.CommandText = @"
			INSERT INTO RouletteGames (CompletedAtUtc, SpinNumber, SpinColour)
			VALUES (@completedAtUtc, @spinNumber, @spinColour);
			SELECT last_insert_rowid();
			";

		gameCommand.Parameters.AddWithValue("@completedAtUtc", completedAtUtc.UtcDateTime.ToString("O"));
		gameCommand.Parameters.AddWithValue("@spinNumber", spinResult.Number);
		gameCommand.Parameters.AddWithValue("@spinColour", spinResult.Colour.ToString());

		var gameId = Convert.ToInt64(await gameCommand.ExecuteScalarAsync());

		await using var outcomeCommand = connection.CreateCommand();
		outcomeCommand.Transaction = transaction;
		outcomeCommand.CommandText = @"
			INSERT INTO RoulettePlayerResults
			(
				GameId,
				UserId,
				BetAmount,
				BetType,
				BetNumber,
				IsWin,
				Payout,
				NetChange,
				BalanceAfterGame
			)
			VALUES
			(
				@gameId,
				@userId,
				@betAmount,
				@betType,
				@betNumber,
				@isWin,
				@payout,
				@netChange,
				(SELECT Balance FROM UserBalances WHERE UserId = @userId)
			);
			";

		var gameIdParameter = outcomeCommand.Parameters.Add("@gameId", SqliteType.Integer);
		var userIdParameter = outcomeCommand.Parameters.Add("@userId", SqliteType.Integer);
		var betAmountParameter = outcomeCommand.Parameters.Add("@betAmount", SqliteType.Integer);
		var betTypeParameter = outcomeCommand.Parameters.Add("@betType", SqliteType.Text);
		var betNumberParameter = outcomeCommand.Parameters.Add("@betNumber", SqliteType.Integer);
		var isWinParameter = outcomeCommand.Parameters.Add("@isWin", SqliteType.Integer);
		var payoutParameter = outcomeCommand.Parameters.Add("@payout", SqliteType.Integer);
		var netChangeParameter = outcomeCommand.Parameters.Add("@netChange", SqliteType.Integer);

		gameIdParameter.Value = gameId;

		foreach (var outcome in playerResults)
		{
			userIdParameter.Value = (long)outcome.UserId;
			betAmountParameter.Value = outcome.BetAmount;
			betTypeParameter.Value = outcome.BetType.ToString();
			betNumberParameter.Value = outcome.BetNumber is null ? DBNull.Value : outcome.BetNumber.Value;
			isWinParameter.Value = outcome.IsWin ? 1 : 0;
			payoutParameter.Value = outcome.Payout;
			netChangeParameter.Value = outcome.NetChange;

			await outcomeCommand.ExecuteNonQueryAsync();
		}

		await transaction.CommitAsync();
	}
}
