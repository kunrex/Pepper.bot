using System;

namespace KunalsDiscordBot.Core.Attributes.GameCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireTicTacToeChannelAttribute : Attribute
    {
        public RequireTicTacToeChannelAttribute()
        {
        }
    }
}
