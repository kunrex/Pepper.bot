using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData
{
    public struct BoostData 
    {
        public Boost Boost { get; set; }

        public BoostData(Boost boost)
        {
            Boost = boost;
        }

        public int GetBoost() => Boost is IValueModel ? ((IValueModel)Boost).GetBoostPrecentage() : 100;
        public TimeSpan GetBoostTime() => Boost is IValueModel ? ((IValueModel)Boost).GetBoostTimeSpan() : TimeSpan.FromDays(1);
    }
}
