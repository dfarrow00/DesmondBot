using DesmondBot.Features.Economy.Application;
using Microsoft.Data.Sqlite;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteEconomyRepository : IEconomyRepository
{
	private readonly SqliteConnectionFactory _connectionFactory;
	private const int _initialBalance = 500;

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

	public async Task RemoveBalanceAsync(ulong userId, int amount)
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			UPDATE UserBalances
			SET Balance = Balance - @amount
			WHERE UserId = @userId;
			";

		command.Parameters.AddWithValue("@userId", userId);
		command.Parameters.AddWithValue("@amount", amount);

		await command.ExecuteNonQueryAsync();
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
}
