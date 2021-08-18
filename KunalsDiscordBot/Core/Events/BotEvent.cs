using System;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Events
{
    public class BotEvent : CustomDisposable
    {
        private event EventHandler<BotEventArgs> Event;
        private TimeSpan spanToWait;

        public BotEvent WithEvent(EventHandler<BotEventArgs> _event)
        {
            Event = _event;
            return this;
        }
        public BotEvent WithSpan(TimeSpan span)
        {
            spanToWait = span;
            return this;
        }

        public async void Execute()
        {
            if (Event == null)
                return;

            await Task.Delay(spanToWait);
            Event.Invoke(this, new BotEventArgs { time = spanToWait });

            Dispose();
        }
    }
}
