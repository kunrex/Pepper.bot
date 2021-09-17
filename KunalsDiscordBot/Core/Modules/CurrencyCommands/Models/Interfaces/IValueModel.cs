using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces
{
    public interface IValueModel
    {
        public int MinimumBoost { get; }
        public int MaximumBoost { get; }

        public TimeSpan MinimumTimeSpan { get; }
        public TimeSpan MaximumTimeSpam { get; }

        public virtual int GetBoostPrecentage() => new Random().Next(MinimumBoost, MaximumBoost);
        public virtual TimeSpan GetBoostTimeSpan() => TimeSpan.FromHours(new Random().Next((int)MinimumTimeSpan.TotalHours, (int)MaximumTimeSpam.TotalHours));
    }
}
