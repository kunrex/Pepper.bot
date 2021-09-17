using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData
{
    public struct BoostData 
    {
        public Boost Boost { get; private set; }

        public BoostData(Boost boost)
        {
            Boost = boost;
        }

        public int GetBoost() => Boost is IValueModel ? ((IValueModel)Boost).GetIncrease() : 100;
        public TimeSpan GetBoostTime() => Boost is ITimeSpanValueModel ? ((ITimeSpanValueModel)Boost).GetTimeSpan() : TimeSpan.FromDays(1);
    }
}
