using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteConnectionFactory
{
	private readonly string _connectionString;
	private readonly ILogger<SqliteConnectionFactory> _logger;

	public SqliteConnectionFactory(string databasePath, ILogger<SqliteConnectionFactory> logger)
	{
		_logger = logger;

		var directoryPath = Path.GetDirectoryName(databasePath);
		if (string.IsNullOrWhiteSpace(directoryPath) == false && Directory.Exists(directoryPath) == false)
		{
			Directory.CreateDirectory(directoryPath);
			_logger.LogInformation("Created database directory at {DirectoryPath}", directoryPath);
		}

		if (File.Exists(databasePath) == false)
		{
			using var _ = File.Create(databasePath);
			_logger.LogInformation("Created database file at {DatabasePath}", databasePath);
		}

		var connectionStringBuilder = new SqliteConnectionStringBuilder
		{
			DataSource = databasePath
		};

		_connectionString = connectionStringBuilder.ConnectionString;
		_logger.LogDebug("Configured SQLite connection for {DatabasePath}", databasePath);
	}

	public SqliteConnection CreateConnection()
	{
		return new SqliteConnection(_connectionString);
	}
}
