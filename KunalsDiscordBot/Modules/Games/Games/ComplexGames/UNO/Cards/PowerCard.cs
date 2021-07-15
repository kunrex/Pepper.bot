using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public enum PowerType
    {
        plus2,
        Skip,
        Reverse,
        plus4,
        Wild
    }

    public class PowerCard : Card
    {
        public PowerType powerType { get; private set; }

        public PowerCard(CardType type, CardColor color, PowerType power) : base(type, color)
        {
            powerType = power;

            fileName = GetFileName();
            if ((power == PowerType.plus4 || power == PowerType.Wild) && color != CardColor.none)
                throw new Exception($"Invalid color for {power} card");
        }

        protected override string GetFileName() => $"{(cardColor == CardColor.none ? "" : cardColor.ToString())}{powerType.ToString().Replace("plus", "+")}.png";
    }
}
