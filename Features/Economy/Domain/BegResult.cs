namespace DesmondBot.Features.Economy.Domain;

public enum BegResultStatus
{
	Success,
	CooldownActive,
	BalanceTooHigh
}

public class BegResult
{
	public required BegResultStatus Status { get; init; }
	public int Amount { get; init; }
	public required DateTimeOffset NextAvailableAtUtc { get; init; }
	public int? MaxAllowedBalance { get; init; }
}
