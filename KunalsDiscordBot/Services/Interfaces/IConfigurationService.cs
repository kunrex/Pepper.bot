using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Services.Configuration
{
    public interface IConfigurationService
    {
        public Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms);
        public Task<DiscordEmbedBuilder> GetPepperBotInfo(int guildCount, int shardCount, int shardId);
    }
}
