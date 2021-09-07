using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts
{
    public class LandmineBoost : Boost, ITheftProtection, IValueBoost
    {
        private readonly int miminumBoost;
        public int MinimumBoost => miminumBoost;

        private readonly int maximumBoost;
        public int MaximumBoost => maximumBoost;

        private readonly TimeSpan minimumTimeSpan;
        public TimeSpan MinimumTimeSpan => minimumTimeSpan;

        private readonly TimeSpan maximumTimeSpam;
        public TimeSpan MaximumTimeSpam => maximumTimeSpam;

        public int Order => 1;
        public int MethodId => 2;

        public LandmineBoost(string _name, int _minimumBoost, int _maximumBoost, TimeSpan _minimumTimeSpan, TimeSpan _maximumTimeSpan) : base(_name)
        {
            miminumBoost = _minimumBoost;
            maximumBoost = _maximumBoost;

            minimumTimeSpan = _minimumTimeSpan;
            maximumTimeSpam = _maximumTimeSpan;
        }

        public LandmineBoost(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) : base(_name, _percentageIncrease, _span, _start)
        {

        }

        protected override Boost CreateClone(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) => new LandmineBoost(_name, _percentageIncrease, _span, _start);

        public Task<UseResult> Use(Profile userProfile, IProfileService profileService) => throw new InvalidOperationException();

        public async Task<UseResult> Use(Profile userProfile, Profile robberProfile, IProfileService profileService)
        {
            var failed = new Random().Next(1, 100) < 50;
            string message = string.Empty;

            if (failed)
            {
                var casted = (ulong)robberProfile.Id;

                await profileService.ModifyProfile(robberProfile, async (x) =>
                {
                    x.Coins = 0;
                    foreach (var boost in await profileService.GetBoosts(casted))
                        await profileService.AddOrRemoveBoost(casted, boost.Name, 0, TimeSpan.FromSeconds(0), "", -1);
                });

                message = $"Well you tried to rob someone, who was smart enough to place a landmin down. Long story short, ya died";
            }

            await profileService.AddOrRemoveBoost(userProfile, Name, 0, TimeSpan.FromSeconds(0), "", -1);
            return new UseResult
            {
                useComplete = true,
                message = message
            };
        }
    }
}
