using Microsoft.Data.Sqlite;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteConnectionFactory
{
	private readonly string _connectionString;

	public SqliteConnectionFactory(string databasePath)
	{
		var connectionStringBuilder = new SqliteConnectionStringBuilder
		{
			DataSource = databasePath
		};

		_connectionString = connectionStringBuilder.ConnectionString;
	}

	public SqliteConnection CreateConnection()
	{
		return new SqliteConnection(_connectionString);
	}
}
