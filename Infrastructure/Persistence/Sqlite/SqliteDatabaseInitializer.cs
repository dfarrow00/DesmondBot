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

		await using var createCommand = connection.CreateCommand();
		createCommand.CommandText = @"
			CREATE TABLE IF NOT EXISTS UserBalances
			(
				UserId INTEGER PRIMARY KEY,
				Balance INTEGER NOT NULL,
				PeakBalance INTEGER NOT NULL DEFAULT 200,
				BegCooldownUntilUtc TEXT NULL
			);

			CREATE TABLE IF NOT EXISTS RouletteGames
			(
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				CompletedAtUtc TEXT NOT NULL,
				SpinNumber INTEGER NOT NULL,
				SpinColour TEXT NOT NULL
			);

			CREATE TABLE IF NOT EXISTS RoulettePlayerResults
			(
				Id INTEGER PRIMARY KEY AUTOINCREMENT,
				GameId INTEGER NOT NULL,
				UserId INTEGER NOT NULL,
				BetAmount INTEGER NOT NULL,
				BetType TEXT NOT NULL,
				BetNumber INTEGER NULL,
				IsWin INTEGER NOT NULL,
				Payout INTEGER NOT NULL,
				NetChange INTEGER NOT NULL,
				BalanceAfterGame INTEGER NOT NULL,
				FOREIGN KEY(GameId) REFERENCES RouletteGames(Id)
			);";

		await createCommand.ExecuteNonQueryAsync();
	}
}
