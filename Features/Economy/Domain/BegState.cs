namespace DesmondBot.Features.Economy.Domain;

public class BegState
{
	public required int Balance { get; init; }
	public required DateTimeOffset? BegCooldownUntilUtc { get; init; }
}
