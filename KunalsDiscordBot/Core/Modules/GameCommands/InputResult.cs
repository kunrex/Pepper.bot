using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public struct InputResult
    {
        public enum ResultType
        {
            End,
            Afk,
            Valid,
            InValid
        }

        public bool WasCompleted { get; set; }
        public ResultType Type;
        public Coordinate Ordinate { get; set; }
        public List<Card> Cards { get; set; }
    }
}
