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

	public async Task<RouletteLeaderboard> GetLeaderboardAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		var biggestWin = await GetLeaderboardEntriesAsync(connection, @"
			SELECT
				ub.UserId,
				COALESCE(MAX(CASE WHEN rpr.NetChange > 0 THEN rpr.NetChange + rpr.BetAmount ELSE 0 END), 0) AS Value
			FROM UserBalances ub
			LEFT JOIN RoulettePlayerResults rpr ON rpr.UserId = ub.UserId
			GROUP BY ub.UserId
			ORDER BY Value DESC, ub.UserId ASC;
			");

		var biggestLoss = await GetLeaderboardEntriesAsync(connection, @"
			SELECT
				ub.UserId,
				COALESCE(MIN(CASE WHEN rpr.NetChange < 0 THEN rpr.NetChange ELSE 0 END), 0) AS Value
			FROM UserBalances ub
			LEFT JOIN RoulettePlayerResults rpr ON rpr.UserId = ub.UserId
			GROUP BY ub.UserId
			ORDER BY Value ASC, ub.UserId ASC;
			");

		var longestWinStreak = await GetLongestWinStreakLeaderboardAsync(connection);

		return new RouletteLeaderboard
		{
			BiggestWin = biggestWin,
			BiggestLoss = biggestLoss,
			LongestWinStreak = longestWinStreak
		};
	}

	public async Task<RoulettePlayerStats> GetPlayerStatsAsync(ulong userId)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var aggregateCommand = connection.CreateCommand();
		aggregateCommand.CommandText = @"
			SELECT
				COUNT(*),
				COALESCE(SUM(CASE WHEN IsWin = 1 THEN 1 ELSE 0 END), 0),
				COALESCE(SUM(BetAmount), 0),
				COALESCE((SELECT PeakBalance FROM UserBalances WHERE UserId = @userId), 200),
				COALESCE(MAX(CASE WHEN NetChange > 0 THEN NetChange + BetAmount ELSE 0 END), 0),
				COALESCE(MIN(CASE WHEN NetChange < 0 THEN NetChange ELSE 0 END), 0)
			FROM RoulettePlayerResults
			WHERE UserId = @userId;
			";

		aggregateCommand.Parameters.AddWithValue("@userId", userId);

		await using var aggregateReader = await aggregateCommand.ExecuteReaderAsync();
		await aggregateReader.ReadAsync();

		var totalGamesPlayed = aggregateReader.GetInt32(0);
		var totalWins = aggregateReader.GetInt32(1);
		var totalWagered = aggregateReader.GetInt32(2);
		var peakBalance = aggregateReader.GetInt32(3);
		var biggestWin = aggregateReader.GetInt32(4);
		var biggestLoss = aggregateReader.GetInt32(5);
		var winRate = totalGamesPlayed == 0 ? 0m : (decimal)totalWins / totalGamesPlayed * 100m;

		await using var streakCommand = connection.CreateCommand();
		streakCommand.CommandText = @"
			SELECT IsWin
			FROM RoulettePlayerResults
			WHERE UserId = @userId
			ORDER BY GameId ASC;
			";

		streakCommand.Parameters.AddWithValue("@userId", userId);

		await using var streakReader = await streakCommand.ExecuteReaderAsync();

		var currentStreak = 0;
		var longestStreak = 0;
		while (await streakReader.ReadAsync())
		{
			var isWin = streakReader.GetInt32(0) == 1;
			if (isWin)
			{
				currentStreak++;
				if (currentStreak > longestStreak)
				{
					longestStreak = currentStreak;
				}
			}
			else
			{
				currentStreak = 0;
			}
		}

		return new RoulettePlayerStats
		{
			TotalGamesPlayed = totalGamesPlayed,
			WinRate = winRate,
			TotalWagered = totalWagered,
			PeakBalance = peakBalance,
			BiggestWin = biggestWin,
			BiggestLoss = biggestLoss,
			LongestWinningStreak = longestStreak,
			CurrentWinningStreak = currentStreak
		};
	}

	private static async Task<List<RouletteLeaderboardEntry>> GetLeaderboardEntriesAsync(SqliteConnection connection, string commandText)
	{
		await using var command = connection.CreateCommand();
		command.CommandText = commandText;

		await using var reader = await command.ExecuteReaderAsync();
		var leaderboard = new List<RouletteLeaderboardEntry>();

		while (await reader.ReadAsync())
		{
			leaderboard.Add(new RouletteLeaderboardEntry
			{
				UserId = (ulong)reader.GetInt64(0),
				Value = reader.GetInt32(1)
			});
		}

		return leaderboard;
	}

	private static async Task<List<RouletteLeaderboardEntry>> GetLongestWinStreakLeaderboardAsync(SqliteConnection connection)
	{
		var userIds = new List<ulong>();

		await using (var userCommand = connection.CreateCommand())
		{
			userCommand.CommandText = @"
				SELECT UserId
				FROM UserBalances
				ORDER BY UserId ASC;
				";

			await using var userReader = await userCommand.ExecuteReaderAsync();
			while (await userReader.ReadAsync())
			{
				userIds.Add((ulong)userReader.GetInt64(0));
			}
		}

		var longestWinStreakByUserId = userIds.ToDictionary(userId => userId, _ => 0);

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT UserId, IsWin
			FROM RoulettePlayerResults
			ORDER BY UserId ASC, GameId ASC;
			";

		await using var reader = await command.ExecuteReaderAsync();

		ulong? currentUserId = null;
		var currentStreak = 0;
		var longestStreak = 0;

		while (await reader.ReadAsync())
		{
			var userId = (ulong)reader.GetInt64(0);
			var isWin = reader.GetInt32(1) == 1;

			if (currentUserId != userId)
			{
				if (currentUserId.HasValue)
				{
					longestWinStreakByUserId[currentUserId.Value] = longestStreak;
				}

				currentUserId = userId;
				currentStreak = 0;
				longestStreak = 0;
			}

			if (isWin)
			{
				currentStreak++;
				if (currentStreak > longestStreak)
				{
					longestStreak = currentStreak;
				}
			}
			else
			{
				currentStreak = 0;
			}
		}

		if (currentUserId.HasValue)
		{
			longestWinStreakByUserId[currentUserId.Value] = longestStreak;
		}

		return longestWinStreakByUserId
			.Select(entry => new RouletteLeaderboardEntry { UserId = entry.Key, Value = entry.Value })
			.OrderByDescending(entry => entry.Value)
			.ThenBy(entry => entry.UserId)
			.ToList();
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
