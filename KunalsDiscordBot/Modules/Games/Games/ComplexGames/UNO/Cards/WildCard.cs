using System;
using KunalsDiscordBot.Modules.Games.Complex.UNO;

namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public class WildCard : PowerCard
    {
        public WildCard(CardColor color, CardType type = CardType.Skip) : base(color, type)
        {

        }

        public override CardType stackables => CardType.Wild;
        public override bool Stack(Card card) => false;
    }
}
