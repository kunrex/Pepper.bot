using System;
using System.Collections.Generic;
using KunalsDiscordBot.Modules.Games.UNO.Cards;

namespace KunalsDiscordBot.Modules.Games
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
