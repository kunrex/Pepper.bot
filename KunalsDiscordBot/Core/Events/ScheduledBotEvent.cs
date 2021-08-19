using System;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Events
{
    public class ScheduledBotEvent : CustomDisposable
    {
        private event EventHandler<ScheduledBotEventArgs> Event;
        private TimeSpan spanToWait;

        public ScheduledBotEvent WithEvent(EventHandler<ScheduledBotEventArgs> _event)
        {
            Event = _event;
            return this;
        }
        public ScheduledBotEvent WithSpan(TimeSpan span)
        {
            spanToWait = span;
            return this;
        }

        public async void Execute()
        {
            if (Event == null)
                return;

            await Task.Delay(spanToWait);
            Event.Invoke(this, new ScheduledBotEventArgs { time = spanToWait });

            Dispose();
        }
    }
}
