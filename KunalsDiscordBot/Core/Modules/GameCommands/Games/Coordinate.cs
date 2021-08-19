using System;

namespace KunalsDiscordBot.Core.Modules.GameCommands
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
