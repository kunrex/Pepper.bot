using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO
{
    public enum StackType
    {
        skip,
        reverse,
        cards,
        none
    }

    public struct StackData
    {
        public StackType stackType { get; set; }

        public int stack;
    }
}
