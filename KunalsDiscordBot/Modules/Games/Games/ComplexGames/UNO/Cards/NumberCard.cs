using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class NumberCard : Card
    {
        public int cardNumber { get; private set; }

        public override CardType stackables => CardType.number;

        public NumberCard(CardColor color, int number) : base(color)
        {
            cardType = CardType.number;

            cardNumber = number;
            fileName = GetFileName();
            cardName = GetCardName();

            if (color == CardColor.none)
                throw new Exception("Invalid Color Given to NumberCard");
        }

        protected override string GetFileName() => $"{cardColor}{cardNumber}.png";
        protected override string GetCardName() => $"{cardColor} {cardNumber}";

        public override bool Stack(Card card) => base.Stack(card);
    }
}
