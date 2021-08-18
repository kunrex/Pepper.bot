using System;
namespace KunalsDiscordBot.Core.Events
{
    public class BotEventArgs : EventArgs
    {
        public TimeSpan time { get; set; }
    }
}
