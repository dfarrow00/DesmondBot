namespace DesmondBot.Features.Economy.Application;

public interface IEconomyRepository
{
	Task AddUserAsync(ulong userId);
	Task AddUsersAsync(IEnumerable<ulong> userIds);
	Task<int> GetBalanceAsync(ulong userId);
	Task<List<(ulong UserId, int Balance)>> GetLeaderboardAsync();
	Task<bool> TryRemoveBalanceAsync(ulong userId, int amount);
	Task AddBalanceAsync(ulong userId, int amount);
	Task<bool> TryTransferBalanceAsync(ulong senderId, ulong recipientId, int amount);
	Task SetMinimumBalancesAsync();
}
