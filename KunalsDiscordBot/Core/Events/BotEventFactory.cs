using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Events
{
    public static class BotEventFactory
    {
        public static List<BotEvent> AllEvents = new List<BotEvent>();

        public static BotEvent CreateEvent()
        {
            var _event = new BotEvent();
            AllEvents.Add(_event);

            return _event;
        }

        public static void ClearAllEvents() => AllEvents = new List<BotEvent>();
    }
}
