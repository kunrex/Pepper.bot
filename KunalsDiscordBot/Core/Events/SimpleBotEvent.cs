using System;
namespace KunalsDiscordBot.Core.Events
{
    public class SimpleBotEvent : CustomDisposable
    {
        public delegate void BotEvent();
        public BotEvent Event { get; private set; }

        public SimpleBotEvent()
        {

        }

        public void WithEvent(BotEvent _event) => Event += _event;

        public void Invoke()
        {
            Event?.Invoke();

            Dispose();
        }
    }
}
