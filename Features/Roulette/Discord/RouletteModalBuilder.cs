using DesmondBot.Features.Roulette.Domain;
using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public static class RouletteModalBuilder
{
	public static ModalProperties Create(int balance)
	{
		return new ModalProperties("roulette-bet-submit", "Place Your Bet")
		{
			new LabelProperties("\U0001FA99 Bet Amount",
				new TextInputProperties("roulette-bet-amount", TextInputStyle.Short)
				{
					Placeholder = "500",
					Required = true,
					MinLength = 1,
					MaxLength = 6
				})
			{
				Description = $"Available balance: {balance}"
			},

			 new LabelProperties("Bet Type",
				new StringMenuProperties("roulette-bet-type", new[]
					{
						new StringMenuSelectOptionProperties("Red - 2x payout", $"{RouletteBetType.Red}")
						{
							Emoji = EmojiProperties.Standard("\U0001F534")
						},

						new StringMenuSelectOptionProperties("Black - 2x payout", $"{RouletteBetType.Black}")
						{
							Emoji = EmojiProperties.Standard("\U000026AB")
						},

						new StringMenuSelectOptionProperties("Number - 36x payout", $"{RouletteBetType.Number}")
						{
							Emoji = EmojiProperties.Standard("\U0001F522")
						}
					})
				{
					Placeholder = "Select a bet type",
					MinValues = 1,
					MaxValues = 1,
					Required = true
				}),

			 new LabelProperties("\U0001F522 Number",
				new TextInputProperties("roulette-bet-number", TextInputStyle.Short)
				{
					Placeholder = "0-36",
					Required = false,
					MinLength = 1,
					MaxLength = 2
				})
			{
				Description = "Only required for number bets"
			},
		};
	}
}
