using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces
{
    public interface ITimeSpanValueModel : IValueModel
    {
        public TimeSpan MinimumTimeSpan { get; }
        public TimeSpan MaximumTimeSpam { get; }

        public virtual TimeSpan GetTimeSpan() => TimeSpan.FromHours(new Random().Next((int)MinimumTimeSpan.TotalHours, (int)MaximumTimeSpam.TotalHours));
    }
}
