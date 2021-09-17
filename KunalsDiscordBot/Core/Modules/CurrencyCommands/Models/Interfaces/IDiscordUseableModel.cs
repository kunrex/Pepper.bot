using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces
{
    public interface IDiscordUseableModel : IUsableModel
    {
        public Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member);
    }
}
