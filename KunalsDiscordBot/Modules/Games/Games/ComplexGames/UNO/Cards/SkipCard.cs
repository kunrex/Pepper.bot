using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class SkipCard : PowerCard
    {
        public SkipCard(CardColor color, CardType type = CardType.Skip) : base(color, type)
        {

        }

        public override CardType stackables => CardType.Skip;
        public override bool ValidNextCardCheck(Card card) => card.cardType == CardType.Wild || card.cardType == CardType.plus4 || card.cardType == CardType.Skip || card.cardColor == cardColor;
    }
}
