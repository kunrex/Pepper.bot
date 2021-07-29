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

        public override bool Stack(Card card)
        {
            if ((card.cardType & stackables) != card.cardType)
                return false;

            return ((NumberCard)card).cardNumber == cardNumber;
        }

        public override bool ValidNextCardCheck(Card card)
        {
            if (card.cardType == CardType.plus4 || card.cardType == CardType.Wild)
                return true;

            return card.cardColor == cardColor || (card is NumberCard && (((NumberCard)card).cardNumber == cardNumber));
        }
    }
}
