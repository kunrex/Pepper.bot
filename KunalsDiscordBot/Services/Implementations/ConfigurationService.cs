using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Configurations.Enums;

namespace KunalsDiscordBot.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public readonly Dictionary<ConfigValue, Func<ulong, object>> Functions;
        public readonly IServerService serverService;
        public readonly ServerConfigData configData;
        public readonly ModuleService moduleService;

        public ConfigurationService(PepperConfigurationManager configurationManager, IServerService service, ModuleService _moduleService)
        {
            serverService = service;
            configData = configurationManager.ServerConfigData;
            moduleService = _moduleService;

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

        public Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms)
        {
            var valueSets = Enum.GetValues(typeof(ConfigValueSet));
            List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

            foreach(var set in valueSets)
            {
                var permissions = moduleService.ModuleInfo[(ConfigValueSet)set].Permissions;
                bool enabled = (perms & permissions) == permissions;

                var embed = new DiscordEmbedBuilder().WithTitle($"__{(ConfigValueSet)set}__");

                if((ConfigValueSet)set != ConfigValueSet.General)
                    embed.AddField($"• Enabled:", $"`{enabled}`\nWether or not the module(s) is(are) enabled in this server"
                        + $"{(enabled ? "" : $"\n**Enabling**: Give Pepper the {permissions.FormatePermissions()} permission(s)")}");

                if(enabled && configData.ServerConfigValues.ContainsKey((ConfigValueSet)set))
                    foreach (var value in configData.ServerConfigValues[(ConfigValueSet)set])
                        embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(Functions[value.ConfigData].Invoke(guildId))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

                embeds.Add(embed);
            }

            return Task.FromResult(embeds);
        }
    }
}
