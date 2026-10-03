using DesmondBot.Features.Economy.Application;
using NetCord;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildUserAddHandler : IGuildUserAddGatewayHandler
{
	private readonly EconomyService _economyService;

	public EconomyGuildUserAddHandler(EconomyService economyService)
	{
		_economyService = economyService;
	}

	public async ValueTask HandleAsync(GuildUser user)
	{
		await _economyService.AddUserAsync(user.Id);
	}
}
