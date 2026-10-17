using DesmondBot.Features.Economy.Application;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildCreateHandler : IGuildCreateGatewayHandler
{
	private readonly IEconomyRepository _economyRepository;

	public EconomyGuildCreateHandler(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	public async ValueTask HandleAsync(GuildCreateEventArgs arg)
	{
		if (arg.Guild == null) return;

		var userIds = arg.Guild.Users.Where(u => u.Value.IsBot == false).Select(u => u.Key);
		await _economyRepository.AddUsersAsync(userIds);
	}
}
