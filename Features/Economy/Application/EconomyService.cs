using DesmondBot.Features.Economy.Domain;
using Microsoft.Extensions.Logging;

namespace DesmondBot.Features.Economy.Application;

public class EconomyService
{
	private readonly IEconomyRepository _economyRepository;
	private readonly Random _random = new();

	private readonly TimeSpan _begCooldownDuration = TimeSpan.FromMinutes(10);
	private const int _begMaxAllowedBalance = 100;

	public EconomyService(IEconomyRepository economyRepository)
	{
		_economyRepository = economyRepository;
	}

	public async Task<BegResult> BegAsync(ulong userId)
	{
		var amount = _random.Next(0, 11) * 10;
		var currentTimeUtc = DateTimeOffset.UtcNow;

		var begCooldownTime = await _economyRepository.GetBegCooldownTimeAsync(userId);
		if (begCooldownTime > currentTimeUtc)
		{
			return new BegResult
			{
				Status = BegResultStatus.CooldownActive,
				NextAvailableAtUtc = begCooldownTime
			};
		}

		var currentBalance = await _economyRepository.GetBalanceAsync(userId);
		if (currentBalance >= _begMaxAllowedBalance)
		{
			return new BegResult
			{
				Status = BegResultStatus.BalanceTooHigh,
				NextAvailableAtUtc = currentTimeUtc,
				MaxAllowedBalance = _begMaxAllowedBalance
			};
		}

		var cooldownUntilUtc = currentTimeUtc.Add(_begCooldownDuration);
		await _economyRepository.SetBegCooldownTimeAsync(userId, cooldownUntilUtc);
		await _economyRepository.AddBalanceAsync(userId, amount);

		return new BegResult
		{
			Status = BegResultStatus.Success,
			Amount = amount,
			NextAvailableAtUtc = cooldownUntilUtc
		};
	}
}
