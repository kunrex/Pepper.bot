using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Events
{
    public static class BotEventFactory
    {
        public static List<ScheduledBotEvent> AllEvents = new List<ScheduledBotEvent>();

        public static ScheduledBotEvent CreateScheduledEvent()
        {
            var _event = new ScheduledBotEvent();
            AllEvents.Add(_event);

            return _event;
        }

        public static void ClearAllEvents() => AllEvents = new List<ScheduledBotEvent>();
    }
}
