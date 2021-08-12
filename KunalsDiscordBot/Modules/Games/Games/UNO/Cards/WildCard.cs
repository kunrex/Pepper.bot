using System;

namespace KunalsDiscordBot.Modules.Games.UNO.Cards
{
    public class WildCard : PowerCard, IChangeColorCard
    {
        public WildCard(CardColor color, CardType type = CardType.Skip) : base(color, type)
        {

        }

        public override CardType stackables => CardType.Wild;
        public CardColor colorToChange { get; set; }

        public override bool Stack(Card card) => false;
        public override bool ValidNextCardCheck(Card card) => card.cardType == CardType.Wild || card.cardType == CardType.plus4 || card.cardColor == colorToChange;
    }
}
