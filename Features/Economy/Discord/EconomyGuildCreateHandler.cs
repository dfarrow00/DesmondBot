using Microsoft.Extensions.Logging;
using DesmondBot.Features.Economy.Application;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildCreateHandler : IGuildCreateGatewayHandler
{
	private readonly IEconomyRepository _economyRepository;
	private readonly ILogger<EconomyGuildCreateHandler> _logger;

	public EconomyGuildCreateHandler(IEconomyRepository economyRepository, ILogger<EconomyGuildCreateHandler> logger)
	{
		_economyRepository = economyRepository;
		_logger = logger;
	}

	public async ValueTask HandleAsync(GuildCreateEventArgs arg)
	{
		if (arg.Guild == null) return;

		var userIds = arg.Guild.Users.Where(u => u.Value.IsBot == false).Select(u => u.Key);
		await _economyRepository.AddUsersAsync(userIds);
		_logger.LogInformation("Detected {UserCount} users and added missing entries to database.", userIds.Count());
	}
}
