using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts.Interfaces
{
    public interface IValueBoost
    {
        public int MinimumBoost { get; }
        public int MaximumBoost { get; }

        public TimeSpan MinimumTimeSpan { get; }
        public TimeSpan MaximumTimeSpam { get; }

        public virtual int GetBoostPrecentage() => new Random().Next(MinimumBoost, MaximumBoost);
        public virtual TimeSpan GetBoostTimeSpan() => TimeSpan.FromSeconds(new Random().Next((int)MinimumTimeSpan.TotalHours, (int)MaximumTimeSpam.TotalHours));
    }
}
