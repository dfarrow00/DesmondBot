using DesmondBot.Features.Economy.Application;
using Microsoft.Extensions.Logging;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace DesmondBot.Features.Economy.Discord;

public class EconomyCommandModule : ApplicationCommandModule<ApplicationCommandContext>
{
	private readonly IEconomyRepository _economyRepository;
	private readonly ILogger<EconomyCommandModule> _logger;

	public EconomyCommandModule(IEconomyRepository economyRepository, ILogger<EconomyCommandModule> logger)
	{
		_economyRepository = economyRepository;
		_logger = logger;
	}

	[SlashCommand("money", "Check your current balance")]
	public async Task<InteractionMessageProperties> GetBalanceAsync()
	{
		var balance = await _economyRepository.GetBalanceAsync(Context.User.Id);
		return new InteractionMessageProperties
		{
			Content = $"Balance: {balance}",
			Flags = MessageFlags.Ephemeral
		};
	}

	[SlashCommand("send-money", "Send money to another user")]
	public async Task<InteractionMessageProperties> SendMoneyAsync(GuildUser recipient, int amount)
	{
		if (recipient.Id == Context.User.Id)
		{
			return new InteractionMessageProperties
			{
				Content = "You cannot send money to yourself",
				Flags = MessageFlags.Ephemeral
			};
		}

		if (recipient.IsBot)
		{
			return new InteractionMessageProperties
			{
				Content = "You cannot send money to DesmondBot",
				Flags = MessageFlags.Ephemeral
			};
		}

		if (amount <= 0)
		{
			return new InteractionMessageProperties
			{
				Content = "The amount must be greater than zero",
				Flags = MessageFlags.Ephemeral
			};
		}

		var success = await _economyRepository.TryTransferBalanceAsync(Context.User.Id, recipient.Id, amount);
		if (success == false)
		{
			return new InteractionMessageProperties
			{
				Content = $"You do not have enough money to send {amount}",
				Flags = MessageFlags.Ephemeral
			};
		}

		_logger.LogInformation(
			"/send-money success. Sender={SenderId}, Recipient={RecipientId}, Amount={Amount}",
			Context.User.Id,
			recipient.Id,
			amount);

		return new InteractionMessageProperties
		{
			Content = $"<@{Context.User.Id}> sent :money_with_wings: {amount} to <@{recipient.Id}>"
		};
	}

	[SlashCommand("leaderboard", "View the money leaderboard")]
	public async Task<InteractionMessageProperties> GetLeaderboardAsync()
	{
		var leaderboard = await _economyRepository.GetLeaderboardAsync();
		_logger.LogInformation("/leaderboard requested by {UserId}.", Context.User.Id);

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
