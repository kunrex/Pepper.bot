using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class NumberCard : Card
    {
        public int cardNumber { get; private set; }

        public NumberCard(CardType type, CardColor color, int number) : base(type, color)
        {
            cardNumber = number;
            fileName = GetFileName();
            cardName = GetCardName();

            if (color == CardColor.none)
                throw new Exception("Invalid Color Given to NumberCard");
        }

        protected override string GetFileName() => $"{cardColor}{cardNumber}.png";
        protected override string GetCardName() => $"{cardColor} {cardNumber}";
    }
}
