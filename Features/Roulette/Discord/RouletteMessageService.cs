using DesmondBot.Features.Roulette.Domain;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public class RouletteMessageService
{
	private readonly RestClient _restClient;
	private ulong? _channelId;
	private ulong? _messageId;

	public RouletteMessageService(RestClient restClient)
	{
		_restClient = restClient;
	}

	public void RegisterMessage(ulong channelId, ulong messageId)
	{
		_channelId = channelId;
		_messageId = messageId;
	}

	public async Task UpdateMessageAsync(RouletteGame game)
	{
		if (_channelId is null || _messageId is null)
		{
			throw new InvalidOperationException("No active roulette message is registered.");
		}

		await _restClient.ModifyMessageAsync(
			_channelId.Value,
			_messageId.Value,
			options =>
			{
				options.Embeds =
				[
					RouletteMessageBuilder.CreateEmbed(game)
				];
			});
	}
}
