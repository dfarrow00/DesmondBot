using NetCord;
using NetCord.Rest;

namespace DesmondBot.Features.Roulette.Discord;

public static class RouletteModalBuilder
{
	public static ModalProperties Create()
	{
		return new ModalProperties("roulette-bet-submit", "Place Your Bet")
		{
			new LabelProperties("🪙 Amount",
				new TextInputProperties("roulette:bet:amount", TextInputStyle.Short)
				{
					Placeholder = "500",
					Required = true,
					MinLength = 1,
					MaxLength = 10
				}),

			 new LabelProperties("Bet Type",
				new StringMenuProperties("roulette-bet-type",
					[
						new StringMenuSelectOptionProperties(
							"Red",
							"red")
						{
							Emoji = EmojiProperties.Standard("🔴")
						},

						new StringMenuSelectOptionProperties(
							"Black",
							"black")
						{
							Emoji = EmojiProperties.Standard("⚫")
						},

						new StringMenuSelectOptionProperties(
							"Number",
							"number")
						{
							Emoji = EmojiProperties.Standard("🔢")
						}
					])
				{
					Placeholder = "Select a bet type",
					MinValues = 1,
					MaxValues = 1,
					Required = true
				}),

			 new LabelProperties("🔢 Number (only for number bets)",
				new TextInputProperties("roulette:bet:number", TextInputStyle.Short)
				{
					Placeholder = "0",
					Required = false,
					MinLength = 1,
					MaxLength = 2
				}),
		};
	}
}
