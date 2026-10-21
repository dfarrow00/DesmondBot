using DesmondBot.Features.Economy.Domain;

namespace DesmondBot.Features.Economy.Application;

public interface IEconomyRepository
{
	Task AddUserAsync(ulong userId);
	Task AddUsersAsync(IEnumerable<ulong> userIds);
	Task AddBalanceAsync(ulong userId, int amount);

	Task<int> GetBalanceAsync(ulong userId);
	Task<EconomyLeaderboard> GetLeaderboardAsync();
	Task<DateTimeOffset> GetBegCooldownTimeAsync(ulong userId);
	Task SetBegCooldownTimeAsync(ulong userId, DateTimeOffset cooldownUntilUtc);

	Task<bool> TryRemoveBalanceAsync(ulong userId, int amount);
	Task<bool> TryTransferBalanceAsync(ulong senderId, ulong recipientId, int amount);
	Task SetMinimumBalancesAsync();
}
