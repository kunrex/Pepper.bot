using System;
using System.Threading.Tasks;

using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts.Interfaces
{
    public interface IUsableBoost
    {
        public Task<UseResult> Use(Profile profile, IProfileService profileService);
    }
}
