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
}
