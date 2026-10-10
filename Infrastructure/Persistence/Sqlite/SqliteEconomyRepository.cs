using DesmondBot.Features.Economy.Application;
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

	public async Task<List<(ulong UserId, int Balance)>> GetLeaderboardAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			SELECT UserId, Balance
			FROM UserBalances
			ORDER BY Balance DESC, UserId ASC;
			";

		await using var reader = await command.ExecuteReaderAsync();
		var leaderboard = new List<(ulong UserId, int Balance)>();

		while (await reader.ReadAsync())
		{
			var userId = (ulong)reader.GetInt64(0);
			var balance = reader.GetInt32(1);
			leaderboard.Add((userId, balance));
		}

		return leaderboard;
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
