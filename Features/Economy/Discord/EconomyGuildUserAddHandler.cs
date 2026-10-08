using DesmondBot.Features.Economy.Application;
using NetCord;
using NetCord.Hosting.Gateway;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyGuildUserAddHandler : IGuildUserAddGatewayHandler
{
	private readonly IEconomyRepository _economyRepository;

	public EconomyGuildUserAddHandler(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	public async ValueTask HandleAsync(GuildUser user)
	{
		await _economyRepository.AddUserAsync(user.Id);
	}
}
