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
		_logger.LogInformation("/money requested by {GlobalName}.", Context.User.GlobalName);
		var balance = await _economyRepository.GetBalanceAsync(Context.User.Id);
		return new InteractionMessageProperties
		{
			Content = $":money_with_wings: {balance}",
			Flags = MessageFlags.Ephemeral
		};
	}

	[SlashCommand("send-money", "Send money to another player")]
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
			"/send-money success. Sender={SenderName}, Recipient={RecipientName}, Amount={Amount}",
			Context.User.GlobalName,
			recipient.GlobalName,
			amount);

		return new InteractionMessageProperties
		{
			Content = $"<@{Context.User.Id}> sent :money_with_wings: {amount} to <@{recipient.Id}>"
		};
	}

	[SlashCommand("money-leaderboard", "View the money leaderboard")]
	public async Task<InteractionMessageProperties> GetMoneyLeaderboardAsync()
	{
		_logger.LogInformation("/money-leaderboard requested by {GlobalName}.", Context.User.GlobalName);
		var leaderboard = await _economyRepository.GetLeaderboardAsync();
		var embed = EconomyMessageBuilder.CreateMoneyLeaderboardEmbed(leaderboard);
		return new InteractionMessageProperties().WithEmbeds([embed]);
	}
}
