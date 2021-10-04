using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces
{
    public interface IValueModel
    {
        public int MaximumIncrease { get; }
        public int MinimumIncrease { get; }

        public virtual int GetIncrease() => new Random().Next(MinimumIncrease, MaximumIncrease);
    }
}
