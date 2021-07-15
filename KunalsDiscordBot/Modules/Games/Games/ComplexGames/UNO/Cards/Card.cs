using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO
{
    public enum CardType
    {
        number,
        powerplay,
    }

    public enum CardColor
    {
        red,
        green,
        blue,
        yellow,
        none
    }

    public abstract class Card
    {
        public CardType cardType { get; protected set; }
        public CardColor cardColor { get; protected set; }

        public string fileName { get; protected set; }

        public Card(CardType type, CardColor color)
        {
            cardType = type;
            cardColor = color;
        }

        protected abstract string GetFileName();
    }
}
