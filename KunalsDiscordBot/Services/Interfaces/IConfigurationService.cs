using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.Entities;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using KunalsDiscordBot.Core.Attributes;
using DSharpPlus;

namespace KunalsDiscordBot.Services.Configuration
{
    public interface IConfigurationService
    {
        public Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms);
    }
}
