using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts
{
    public partial class Boost
    {
        public static readonly Boost Padlock = new PadlockBoost("Padlock", 100, 100, TimeSpan.FromHours(10), TimeSpan.FromHours(12));
        public static readonly Boost Landmine = new LandmineBoost("Landmine", 50, 50, TimeSpan.FromHours(5), TimeSpan.FromHours(8));

        public static readonly Boost Alcohol = new LuckBoost("Alcohol", 10, 15, TimeSpan.FromHours(10), TimeSpan.FromHours(15));

        public static readonly Boost Heart = new InvincibilityBoost("Heart", 100, 100, TimeSpan.FromHours(10), TimeSpan.FromHours(10));

        public static readonly List<Boost> AllBoosts = new List<Boost>() { Padlock, Landmine, Alcohol, Heart };
    }
}
