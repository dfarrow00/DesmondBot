namespace DesmondBot.Features.Roulette.Domain;

public enum PlaceBetResult
{
	Success,
	InsufficientBalance,
	BettingClosed,
	NoActiveGame,
	AlreadyBet,
	InvalidAmount,
	NumberRequired,
	InvalidNumber
}
