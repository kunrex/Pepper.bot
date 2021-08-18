using System;

namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public abstract class PowerCard : Card
    {
        public abstract override CardType stackables { get; }

        public PowerCard(CardColor color, CardType power) : base(color)
        {
            cardType = power;

            fileName = GetFileName();
            cardName = GetCardName();

            if ((power == CardType.plus4 || power == CardType.Wild) && color != CardColor.none)
                throw new Exception($"Invalid color for {power} card");
        }

        protected override string GetFileName() => $"{(cardColor == CardColor.none ? "" : cardColor.ToString())}{cardType.ToString().Replace("plus", "+")}.png";
        protected override string GetCardName() => $"{(cardColor == CardColor.none ? "" : cardColor.ToString())} {cardType.ToString().Replace("plus", "+")}";

        public abstract override bool ValidNextCardCheck(Card card);
        public override bool Stack(Card card) => (card.cardType & stackables) == card.cardType;
    }
}
