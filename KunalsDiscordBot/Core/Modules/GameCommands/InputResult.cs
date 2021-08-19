using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public struct InputResult
    {
        public enum Type
        {
            end,
            afk,
            valid,
            inValid
        }

        public bool wasCompleted { get; set; }
        public Type type;
        public Coordinate ordinate { get; set; }
        public List<Card> cards { get; set; }
    }
}
