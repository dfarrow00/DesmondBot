namespace DesmondBot.Features.Economy.Application;

public interface IEconomyRepository
{
	Task AddUserAsync(ulong userId);
	Task AddUsersAsync(IEnumerable<ulong> userIds);
	Task<int> GetBalanceAsync(ulong userId);
	Task<bool> TryRemoveBalanceAsync(ulong userId, int amount);
	Task AddBalanceAsync(ulong userId, int amount);
}
