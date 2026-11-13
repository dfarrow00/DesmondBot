using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace DesmondBot.Infrastructure.Persistence.Sqlite;

public class SqliteBackupService : BackgroundService
{
	private readonly string _backupDirectory;
	private readonly TimeSpan _backupInterval;
	private readonly TimeSpan _retentionPeriod;
	private readonly SqliteConnectionFactory _connectionFactory;
	private readonly ILogger<SqliteBackupService> _logger;

	public SqliteBackupService(SqliteConnectionFactory connectionFactory, ILogger<SqliteBackupService> logger, IConfiguration config)
	{
		_connectionFactory = connectionFactory;
		_logger = logger;

		var backupPath = config.GetValue<string>("Database:BackupPath") ?? throw new InvalidOperationException("Database backup path not found in configuration");
		var backupIntervalHours = config.GetValue<double?>("Database:BackupIntervalHours") ?? throw new InvalidOperationException("Database backup interval not found in configuration"); ;
		var retentionPeriodDays = config.GetValue<double?>("Database:RetentionPeriodDays") ?? throw new InvalidOperationException("Database retention period not found in configuration");

		if (backupIntervalHours <= 0)
			throw new InvalidOperationException("Backup interval must be greater than zero.");

		if (retentionPeriodDays <= 0)
			throw new InvalidOperationException("Retention period must be greater than zero.");

		_backupDirectory = backupPath;
		_backupInterval = TimeSpan.FromHours(backupIntervalHours);
		_retentionPeriod = TimeSpan.FromDays(retentionPeriodDays);
	}

	protected override async Task ExecuteAsync(
	CancellationToken cancellationToken)
	{
		_logger.LogInformation("SQLite backup service started. Backup interval: {BackupIntervalHours} hours, retention: {RetentionDays} days", _backupInterval.TotalHours, _retentionPeriod.TotalDays);

		using var timer = new PeriodicTimer(_backupInterval);

		try
		{
			do
			{
				try
				{
					await CreateBackupAsync(cancellationToken);
					CleanupExpiredBackups();
				}
				catch (Exception ex) when (ex is not OperationCanceledException || cancellationToken.IsCancellationRequested == false)
				{
					_logger.LogError(ex, "Error occurred during automated SQLite backup");
				}

			} 
			while (await timer.WaitForNextTickAsync(cancellationToken));
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			_logger.LogInformation("SQLite backup service is stopping.");
		}
	}

	private async Task CreateBackupAsync(CancellationToken cancellationToken)
	{
		Directory.CreateDirectory(_backupDirectory);

		var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss",CultureInfo.InvariantCulture);

		var backupFileName = $"desmond-backup-{timestamp}.db";
		var backupPath = Path.Combine(_backupDirectory, backupFileName);

		await using var connection = _connectionFactory.CreateConnection();
		await connection.OpenAsync(cancellationToken);

		await using var command = connection.CreateCommand();

		command.CommandText = "VACUUM INTO $backupPath;";
		command.Parameters.AddWithValue("$backupPath", backupPath);
		await command.ExecuteNonQueryAsync(cancellationToken);

		_logger.LogInformation("Created SQLite backup at {BackupPath}", backupPath);
	}

	private void CleanupExpiredBackups()
	{
		var deleteBeforeUtc = DateTime.UtcNow - _retentionPeriod;

		foreach (var backupFile in Directory.EnumerateFiles(_backupDirectory, "desmond-backup-*.db", SearchOption.TopDirectoryOnly))
		{
			if (File.GetLastWriteTimeUtc(backupFile) >= deleteBeforeUtc)
			{
				continue;
			}

			try
			{
				File.Delete(backupFile);
				_logger.LogInformation("Deleted expired SQLite backup {BackupPath}", backupFile);
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to delete expired SQLite backup {BackupPath}", backupFile);
			}
		}
	}
}
