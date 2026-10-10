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

	[SlashCommand("leaderboard", "View the balance leaderboard")]
	public async Task<InteractionMessageProperties> GetLeaderboardAsync()
	{
		var leaderboard = await _economyRepository.GetLeaderboardAsync();
		var description = leaderboard.Count == 0
			? "No balances found."
			: string.Join("\n", leaderboard.Select((entry, index) =>
			{
				var position = index switch
				{
					0 => ":first_place:",
					1 => ":second_place:",
					2 => ":third_place:",
					_ => $"#{index + 1}"
				};

				return $"{position} <@{entry.UserId}> — :money_with_wings: {entry.Balance}";
			}));

		var embed = new EmbedProperties
		{
			Title = ":trophy: Leaderboard",
			Description = description,
			Color = new Color(0xFFD700)
		};

		return new InteractionMessageProperties().WithEmbeds(new[] { embed });
	}
}
