using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Configurations.Enums;

namespace KunalsDiscordBot.Services.Configuration
{
    public interface IConfigurationService
    {
        public Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms);
        public Task<DiscordEmbedBuilder> GetConfigPage(ulong guildId, Permissions perms, ConfigValueSet set);

        public Task GeneratePepperInfoMessage(PepperBot shard, DiscordChannel channel);
        public Task GeneratePepperInfoMessage(PepperBot shard, DiscordChannel channel, ulong messageId, string userName);
    }
}
