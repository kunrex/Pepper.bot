using System;

namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class Plus4Card : PowerCard, IStackable
    {
        public Plus4Card(CardColor color, CardType type = CardType.plus4) : base(color, type)
        {

        }

        public override CardType stackables => CardType.plus2 | CardType.plus4;
    }
}
