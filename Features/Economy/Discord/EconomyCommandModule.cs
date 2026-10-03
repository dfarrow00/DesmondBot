using DesmondBot.Features.Economy.Application;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly IEconomyRepository _economyRepository;

	public EconomyCommandModule(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	[SlashCommand("balance", "Check your current balance")]
	public async Task<InteractionMessageProperties> GetBalanceAsync()
	{
		var balance = await _economyRepository.GetBalanceAsync(Context.User.Id);
		return new InteractionMessageProperties
		{
			Content = $"Balance: {balance}",
			Flags = MessageFlags.Ephemeral
		};
	}
}
