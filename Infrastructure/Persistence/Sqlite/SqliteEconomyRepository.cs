using DesmondBot.Features.Economy.Application;
using DesmondBot.Features.Economy.Domain;
using Microsoft.Data.Sqlite;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteEconomyRepository : IEconomyRepository
{
	private readonly SqliteConnectionFactory _connectionFactory;
	private const int _initialBalance = 200;

	public SqliteEconomyRepository(SqliteConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task AddUserAsync(ulong userId)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
            INSERT INTO UserBalances (UserId, Balance)
            VALUES (@userId, @balance)
            ON CONFLICT(UserId) DO NOTHING;
            ";

		command.Parameters.AddWithValue("@userId", userId);
		command.Parameters.AddWithValue("@balance", _initialBalance);

		await command.ExecuteNonQueryAsync();
	}

	public async Task AddUsersAsync(IEnumerable<ulong> userIds)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var transaction = connection.BeginTransaction();

		await using var command = connection.CreateCommand();

		command.CommandText = @"
			INSERT INTO UserBalances (UserId, Balance)
			VALUES (@userId, @balance)
			ON CONFLICT(UserId) DO NOTHING;
        ";

		command.Transaction = transaction;

		var userIdParameter = command.Parameters.Add("@userId", SqliteType.Integer);
		var balanceParameter = command.Parameters.Add("@balance", SqliteType.Integer);

		balanceParameter.Value = _initialBalance;

		foreach (var userId in userIds)
		{
			userIdParameter.Value = (long)userId;

			await command.ExecuteNonQueryAsync();
		}

		await transaction.CommitAsync();
	}

	public async Task<int> GetBalanceAsync(ulong userId)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT Balance
			FROM UserBalances
			WHERE UserId = @userId;
			";

		command.Parameters.AddWithValue("@userId", userId);

		var result = await command.ExecuteScalarAsync();
		return Convert.ToInt32(result);
	}

	public async Task<EconomyLeaderboard> GetLeaderboardAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		var currentBalance = await GetLeaderboardEntriesAsync(connection, @"
			SELECT UserId, Balance
			FROM UserBalances
			ORDER BY Balance DESC, UserId ASC;
			");

		//TODO: Domain seperation issue here by querying the RoulettePlayerResults table in the economy repository.
		var peakBalance = await GetLeaderboardEntriesAsync(connection, @"
			SELECT
				ub.UserId,
				CASE
					WHEN COALESCE(MAX(rpr.BalanceAfterGame), 0) < 200 THEN 200
					ELSE MAX(rpr.BalanceAfterGame)
				END AS Value
			FROM UserBalances ub
			LEFT JOIN RoulettePlayerResults rpr ON rpr.UserId = ub.UserId
			GROUP BY ub.UserId
			ORDER BY Value DESC, ub.UserId ASC;
			");

		var biggestWin = await GetLeaderboardEntriesAsync(connection, @"
			SELECT
				ub.UserId,
				COALESCE(MAX(CASE WHEN rpr.NetChange > 0 THEN rpr.NetChange + rpr.BetAmount ELSE 0 END), 0) AS Value
			FROM UserBalances ub
			LEFT JOIN RoulettePlayerResults rpr ON rpr.UserId = ub.UserId
			GROUP BY ub.UserId
			ORDER BY Value DESC, ub.UserId ASC;
			");

		var longestWinStreak = await GetLongestWinStreakLeaderboardAsync(connection, currentBalance);

		return new EconomyLeaderboard
		{
			CurrentBalance = currentBalance,
			PeakBalance = peakBalance,
			LongestWinStreak = longestWinStreak,
			BiggestWin = biggestWin
		};
	}

	private static async Task<List<LeaderboardEntry>> GetLeaderboardEntriesAsync(SqliteConnection connection, string commandText)
	{
		await using var command = connection.CreateCommand();
		command.CommandText = commandText;

		await using var reader = await command.ExecuteReaderAsync();
		var leaderboard = new List<LeaderboardEntry>();

		while (await reader.ReadAsync())
		{
			leaderboard.Add(new LeaderboardEntry
			{
				UserId = (ulong)reader.GetInt64(0),
				Value = reader.GetInt32(1)
			});
		}

		return leaderboard;
	}

	private static async Task<List<LeaderboardEntry>> GetLongestWinStreakLeaderboardAsync(
		SqliteConnection connection,
		List<LeaderboardEntry> currentBalance)
	{
		var longestWinStreakByUserId = currentBalance.ToDictionary(entry => entry.UserId, _ => 0);

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
			.Select(entry => new LeaderboardEntry { UserId = entry.Key, Value = entry.Value })
			.OrderByDescending(entry => entry.Value)
			.ThenBy(entry => entry.UserId)
			.ToList();
	}

	public async Task<bool> TryRemoveBalanceAsync(ulong userId, int amount)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			UPDATE UserBalances
			SET Balance = Balance - @amount
			WHERE UserId = @userId
			AND Balance >= @amount;
			";

		command.Parameters.AddWithValue("@userId", userId);
		command.Parameters.AddWithValue("@amount", amount);

		var rowsAffected = await command.ExecuteNonQueryAsync();
		return rowsAffected == 1;
	}

	public async Task AddBalanceAsync(ulong userId, int amount)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			UPDATE UserBalances
			SET Balance = Balance + @amount
			WHERE UserId = @userId;
			";

		command.Parameters.AddWithValue("@userId", userId);
		command.Parameters.AddWithValue("@amount", amount);

		await command.ExecuteNonQueryAsync();
	}

	public async Task<bool> TryTransferBalanceAsync(ulong senderId, ulong recipientId, int amount)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var transaction = connection.BeginTransaction();

		await using var debitCommand = connection.CreateCommand();
		debitCommand.Transaction = transaction;
		debitCommand.CommandText = @"
			UPDATE UserBalances
			SET Balance = Balance - @amount
			WHERE UserId = @senderId
			AND Balance >= @amount;
			";

		debitCommand.Parameters.AddWithValue("@senderId", senderId);
		debitCommand.Parameters.AddWithValue("@amount", amount);

		var rowsAffected = await debitCommand.ExecuteNonQueryAsync();
		if (rowsAffected < 1)
		{
			await transaction.RollbackAsync();
			return false;
		}

		await using var creditCommand = connection.CreateCommand();
		creditCommand.Transaction = transaction;
		creditCommand.CommandText = @"
			UPDATE UserBalances
			SET Balance = Balance + @amount
			WHERE UserId = @recipientId;
			";

		creditCommand.Parameters.AddWithValue("@recipientId", recipientId);
		creditCommand.Parameters.AddWithValue("@amount", amount);

		await creditCommand.ExecuteNonQueryAsync();
		await transaction.CommitAsync();
		return true;
	}

	public async Task SetMinimumBalancesAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			UPDATE UserBalances
			SET Balance = @minimumBalance
			WHERE Balance < @minimumBalance;
			";

		command.Parameters.AddWithValue("@minimumBalance", _initialBalance);

		await command.ExecuteNonQueryAsync();
	}
}
