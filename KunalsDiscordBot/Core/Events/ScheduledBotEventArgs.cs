using System;
namespace KunalsDiscordBot.Core.Events
{
    public class ScheduledBotEventArgs : EventArgs
    {
        public TimeSpan time { get; set; }
    }
}
