using System;

namespace KunalsDiscordBot.Modules.Games
{
    public partial struct Coordinate
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
