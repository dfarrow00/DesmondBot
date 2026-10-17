using DesmondBot.Features.Economy.Application;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildUserAddHandler : IGuildUserAddGatewayHandler
{
	private readonly IEconomyRepository _economyRepository;
	private readonly ILogger<EconomyGuildUserAddHandler> _logger;

	public EconomyGuildUserAddHandler(IEconomyRepository economyRepository, ILogger<EconomyGuildUserAddHandler> logger)
	{
		_economyRepository = economyRepository;
		_logger = logger;
	}

	public async ValueTask HandleAsync(GuildUser user)
	{
		await _economyRepository.AddUserAsync(user.Id);
		_logger.LogInformation("Added user {GlobalName} to the database.", user.GlobalName);
	}
}
