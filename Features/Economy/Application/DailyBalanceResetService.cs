using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DesmondBot.Features.Economy.Application;

public class DailyBalanceResetService : IHostedService
{
	private readonly IEconomyRepository _economyRepository;
	private readonly ILogger<DailyBalanceResetService> _logger;
	private Timer? _timer;

	public DailyBalanceResetService(IEconomyRepository economyRepository, ILogger<DailyBalanceResetService> logger)
	{
		_economyRepository = economyRepository;
		_logger = logger;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var midnight = now.Date.AddDays(1);
		var timeUntilMidnight = midnight - now;

		_timer = new Timer(
			callback: async _ => await ExecuteResetAsync(),
			state: null,
			dueTime: timeUntilMidnight,
			period: TimeSpan.FromHours(24));

		_logger.LogInformation("Daily balance reset scheduled to run at {MidnightTime} UTC", midnight);

		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Daily balance reset service stopping...");
		_timer?.Dispose();
		return Task.CompletedTask;
	}

	private async Task ExecuteResetAsync()
	{
		try
		{
			_logger.LogInformation("Starting daily balance reset...");

			await _economyRepository.SetMinimumBalancesAsync();

			_logger.LogInformation("Daily balance reset completed successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred during daily balance reset");
		}
	}
}
