using DesmondBot.Features.Roulette.Domain;

namespace DesmondBot.Features.Roulette.Application;

public interface IRouletteMessageService
{
	void RegisterMessage(ulong channelId, ulong messageId);
	void ResetMessage();
	Task UpdateStartMessageAsync(RouletteGame game);
	Task UpdateMessageBettingClosedAsync(RouletteGame game);
	Task UpdateMessageResultAsync(RouletteGame game, RouletteResult resultDisplay);
}
