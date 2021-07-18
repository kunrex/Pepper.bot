using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class ReverseCard : PowerCard
    {
        public ReverseCard(CardColor color, CardType type = CardType.Reverse) : base(color, type)
        {

        }

        public override CardType stackables => CardType.Reverse;
    }
}
