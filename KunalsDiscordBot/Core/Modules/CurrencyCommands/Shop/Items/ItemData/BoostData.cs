using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items
{
    public struct BoostData 
    {
        public Boost Boost { get; set; }

        public BoostData(Boost boost)
        {
            Boost = boost;
        }

        public int GetBoost() => Boost is IValueBoost ? ((IValueBoost)Boost).GetBoostPrecentage() : 100;
        public TimeSpan GetBoostTime() => Boost is IValueBoost ? ((IValueBoost)Boost).GetBoostTimeSpan() : TimeSpan.FromDays(1);
    }
}
