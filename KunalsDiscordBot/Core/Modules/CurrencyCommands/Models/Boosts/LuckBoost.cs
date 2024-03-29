﻿using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts
{
    public class LuckBoost : Boost, ITimeSpanValueModel
    {
        public LuckBoost(string _name, int _minimumBoost, int _maximumBoost, TimeSpan _minimumTimeSpan, TimeSpan _maximumTimeSpan) : base(_name)
        {
            miminumBoost = _minimumBoost;
            maximumBoost = _maximumBoost;

            minimumTimeSpan = _minimumTimeSpan;
            maximumTimeSpam = _maximumTimeSpan;
        }

        public LuckBoost(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) : base(_name, _percentageIncrease, _span, _start)
        {
            
        }

        protected override Boost CreateClone(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) => new LuckBoost(_name, _percentageIncrease, _span, _start);

        private readonly int miminumBoost;
        public int MinimumIncrease => miminumBoost;

        private readonly int maximumBoost;
        public int MaximumIncrease => maximumBoost;

        private readonly TimeSpan minimumTimeSpan;
        public TimeSpan MinimumTimeSpan => minimumTimeSpan;

        private readonly TimeSpan maximumTimeSpam;
        public TimeSpan MaximumTimeSpam => maximumTimeSpam;
    }
}
