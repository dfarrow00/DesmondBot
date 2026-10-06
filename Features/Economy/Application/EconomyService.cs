namespace DesmondBot.Features.Economy.Application;

public class EconomyService
{
	private readonly IEconomyRepository _economyRepository;

	public EconomyService(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	public async Task AddUserAsync(ulong userId)
	{
		await _economyRepository.AddUserAsync(userId);
	}

	public async Task AddUsersAsync(IEnumerable<ulong> userIds)
	{
		await _economyRepository.AddUsersAsync(userIds);
	}

	public async Task<int> GetBalanceAsync(ulong userId)
	{
		//Improvement: If user doesnt exist in db, create first. Could cache result to minimize unnecessary reads
		return await _economyRepository.GetBalanceAsync(userId);
	}

	public async Task RemoveBalanceAsync(ulong userId, int amount)
	{
		await _economyRepository.RemoveBalanceAsync(userId, amount);
	}
}
