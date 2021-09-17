using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Profile;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools
{
    public abstract partial class Tool : IDiscordUseableModel
    {
        public string Name { get; protected set; }

        public Tool(string name)
        {
            Name = name;
        }

        public abstract Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member);
        public virtual Task<UseResult> Use(Profile profile, IProfileService profileService) => throw new NotImplementedException();
    }
}
