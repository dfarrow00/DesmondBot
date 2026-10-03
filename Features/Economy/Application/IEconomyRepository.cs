namespace DesmondBot.Features.Economy.Application;

public interface IEconomyRepository
{
	Task AddUserAsync(ulong userId);
	Task AddUsersAsync(IEnumerable<ulong> userIds);
}
