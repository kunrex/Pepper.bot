using System;
using System.Threading.Tasks;

using KunalsDiscordBot.Services.Currency;
using DiscordBotDataBase.Dal.Models.Profile;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts.Interfaces
{
    public interface ITheftProtection : IUsableModel
    {
        public int Order { get; }
        public int MethodId { get; }

        public new Task<UseResult> Use(Profile userProfile, IProfileService profileService);
        public Task<UseResult> Use(Profile userProfile, Profile robberProfile, IProfileService profileService);
    }
}
