using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class Plus2Card : PowerCard
    {
        public Plus2Card(CardColor color, CardType type = CardType.plus2) : base(color, type)
        {

        }

        public override CardType stackables => CardType.plus2;
    }
}
