using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces
{
    public interface IUsableModel
    {
        public Task<UseResult> Use(Profile profile, IProfileService profileService);
    }
}
