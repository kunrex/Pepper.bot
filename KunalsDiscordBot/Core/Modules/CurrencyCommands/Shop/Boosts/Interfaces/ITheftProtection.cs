using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts.Interfaces
{
    public interface ITheftProtection : IUsableBoost
    {
        public int Order { get; }

        public new Task<UseResult> Use(Profile userProfile, IProfileService profileService);
        public Task<UseResult> Use(Profile userProfile, Profile robberProfile, IProfileService profileService);
    }
}
