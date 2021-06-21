using System;
namespace KunalsDiscordBot.Modules.Games.Complex.Battleship
{
    public class BattleShipCoOrdinate : CoOrdinate
    {
        public enum OrdinateType
        {
            free,
            hit,
            ship,
            shipHit,
        }

        public OrdinateType type;
    }
}
