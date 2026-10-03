namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteDatabaseInitializer
{
	private readonly SqliteConnectionFactory _connectionFactory;

	public SqliteDatabaseInitializer(SqliteConnectionFactory connectionFactory)
	{
		_connectionFactory = connectionFactory;
	}

	public async Task InitializeAsync()
	{
		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync();

		await using var command = connection.CreateCommand();
		command.CommandText = @"
			CREATE TABLE IF NOT EXISTS UserBalances
            (
                UserId INTEGER PRIMARY KEY,
                Balance INTEGER NOT NULL
            );";

		await command.ExecuteNonQueryAsync();
	}
}
