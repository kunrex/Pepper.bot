using System;
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
    }
}
