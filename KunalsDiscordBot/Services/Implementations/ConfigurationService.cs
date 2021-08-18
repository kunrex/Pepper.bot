using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Services.General;

namespace KunalsDiscordBot.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public readonly Dictionary<ConfigValue, Func<ulong, object>> Functions;
        public readonly IServerService serverService;
        public readonly ServerConfigData configData;

        public ConfigurationService(PepperConfigurationManager configurationManager, IServerService service)
        {
            serverService = service;
            configData = configurationManager.ServerConfigData;

            Functions = new Dictionary<ConfigValue, Func<ulong, object>>()
            {
                 { ConfigValue.EnforcePermissions, (id) => serverService.GetServerProfile(id).GetAwaiter().GetResult().RestrictPermissionsToAdmin == 1},
                 { ConfigValue.LogErrors, (id) => serverService.GetServerProfile(id).GetAwaiter().GetResult().LogErrors == 1},
                 { ConfigValue.LogMembers, (id) => serverService.GetServerProfile(id).GetAwaiter().GetResult().LogNewMembers == 1},
                 { ConfigValue.WelcomeChannel, (id) => (ulong)serverService.GetServerProfile(id).GetAwaiter().GetResult().WelcomeChannel},
                 { ConfigValue.RuleChannel, (id) => (ulong)serverService.GetServerProfile(id).GetAwaiter().GetResult().RulesChannelId},
                 { ConfigValue.ModRole, (id) => (ulong)serverService.GetModerationData(id).GetAwaiter().GetResult().ModeratorRoleId},
                 { ConfigValue.MutedRole, (id) => (ulong)serverService.GetModerationData(id).GetAwaiter().GetResult().MutedRoleId},
                 { ConfigValue.RuleCount, (id) => serverService.GetAllRules(id).GetAwaiter().GetResult().Count },
                 { ConfigValue.DJEnfore, (id) => serverService.GetMusicData(id).GetAwaiter().GetResult().UseDJRoleEnforcement == 1},
                 { ConfigValue.DJRole, (id) => (ulong)serverService.GetMusicData(id).GetAwaiter().GetResult().DJRoleId},
                 { ConfigValue.AllowNSFW, (id) => serverService.GetFunData(id).GetAwaiter().GetResult().AllowNSFW == 1},
                 { ConfigValue.AllowSpamCommand, (id) => serverService.GetFunData(id).GetAwaiter().GetResult().AllowSpamCommand == 1},
                 { ConfigValue.AllowGhostCommand, (id) => serverService.GetFunData(id).GetAwaiter().GetResult().AllowGhostCommand == 1},
                 { ConfigValue.Connect4Channel, (id) => (ulong)serverService.GetGameData(id).GetAwaiter().GetResult().Connect4Channel},
                 { ConfigValue.TicTacToeChannel, (id) => (ulong)serverService.GetGameData(id).GetAwaiter().GetResult().TicTacToeChannel},
            };
        }

        public Dictionary<ConfigValue, Func<object, string>> StringConverions = new Dictionary<ConfigValue, Func<object, string>>()
        {
            { ConfigValue.EnforcePermissions, (s) => $"`{(bool)s}`"},
            { ConfigValue.LogErrors, (s) => $"`{(bool)s}`"},
            { ConfigValue.LogMembers, (s) => $"`{(bool)s}`"},
            { ConfigValue.WelcomeChannel, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<#{(ulong)s}>")}"},
            { ConfigValue.RuleChannel, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<#{(ulong)s}>")}"},
            { ConfigValue.ModRole, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<@&{(ulong)s}>")}"},
            { ConfigValue.MutedRole, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<@&{(ulong)s}>")}"},
            { ConfigValue.RuleCount, (s) => $"`{(int)s}`"},
            { ConfigValue.DJEnfore, (s) => $"`{(bool)s}`"},
            { ConfigValue.DJRole, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<@&{(ulong)s}>")}"},
            { ConfigValue.AllowNSFW, (s) => $"`{(bool)s}`"},
            { ConfigValue.AllowSpamCommand, (s) => $"`{(bool)s}`"},
            { ConfigValue.AllowGhostCommand, (s) => $"`{(bool)s}`"},
            { ConfigValue.Connect4Channel, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<#{(ulong)s}>")}"},
            { ConfigValue.TicTacToeChannel, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<#{(ulong)s}>")}"},
        };

        public async Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms) => new List<DiscordEmbedBuilder>()
        {
            await GetGeneralConfigPage(guildId),
            await GetModerationConfigPage(guildId, (perms & Permissions.Administrator) == Permissions.Administrator),
            await GetMusicConfigPage(guildId),
            await GetFunConfigPage(guildId),
            await GetGamesConfigPage(guildId)
        };

        public Task<DiscordEmbedBuilder> GetFunConfigPage(ulong id)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("__Fun__");

            foreach(var value in configData.ServerConfigValues[ConfigValueSet.Fun])
                embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(id))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

            return Task.FromResult(embed);
        }

        public Task<DiscordEmbedBuilder> GetGamesConfigPage(ulong id)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("__Games__");

            foreach (var value in configData.ServerConfigValues[ConfigValueSet.Games])
                embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(id))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

            return Task.FromResult(embed);
        }

        public Task<DiscordEmbedBuilder> GetGeneralConfigPage(ulong id)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("__General__");

            foreach (var value in configData.ServerConfigValues[ConfigValueSet.General])
                embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(id))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

            return Task.FromResult(embed);
        }

        public Task<DiscordEmbedBuilder> GetModerationConfigPage(ulong id, bool hasMod)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "__Moderation and Soft Moderation__",
            }.AddField($"• Enabled:", $"`{hasMod}`\nWether or not the moderation and soft moderation modules are enabled in thi server"
                + $"{(hasMod ? "" : "\n**Enabling**: Give Pepper the `Administrator` permission")}");

            if (hasMod)
            {
                foreach (var value in configData.ServerConfigValues[ConfigValueSet.Moderation])
                    embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(id))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");
            }

            return Task.FromResult(embed);
        }

        public Task<DiscordEmbedBuilder> GetMusicConfigPage(ulong id)
        {
            var embed = new DiscordEmbedBuilder().WithTitle("__Music__");

            foreach (var value in configData.ServerConfigValues[ConfigValueSet.Music])
                embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(id))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

            return Task.FromResult(embed);
        }
    }
}
