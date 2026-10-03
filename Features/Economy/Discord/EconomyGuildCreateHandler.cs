using DesmondBot.Features.Economy.Application;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildCreateHandler : IGuildCreateGatewayHandler
{
	private readonly EconomyService _economyService;

	public EconomyGuildCreateHandler(EconomyService economyService)
	{
		_economyService = economyService;
	}

	public async ValueTask HandleAsync(GuildCreateEventArgs arg)
	{
		var userIds = arg.Guild!.Users.Where(u => u.Value.IsBot == false).Select(u => u.Key);
		await _economyService.AddUsersAsync(userIds);
	}
}
