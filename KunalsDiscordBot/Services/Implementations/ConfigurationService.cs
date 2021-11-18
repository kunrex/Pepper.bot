using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Services.Moderation;

namespace KunalsDiscordBot.Services.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public readonly Dictionary<ConfigValue, Func<ulong, Task<object>>> Functions;

        public readonly IServerService serverService;
        public readonly ServerConfigData configData;
        public readonly IModuleService moduleService;

        public ConfigurationService(PepperConfigurationManager configurationManager, IServerService service, IModuleService _moduleService, IModerationService moderationService)
        {
            serverService = service;
            configData = configurationManager.ServerConfigData;
            moduleService = _moduleService;

            Functions = new Dictionary<ConfigValue, Func<ulong, Task<object>>>()
            {
                 { ConfigValue.EnforcePermissions, async(id) => (await serverService.GetServerProfile(id)).RestrictPermissionsToAdmin == 1},
                 { ConfigValue.LogErrors, async(id) => (await serverService.GetServerProfile(id)).LogErrors == 1},
                 { ConfigValue.LogMembers, async(id) => (await serverService.GetServerProfile(id)).LogNewMembers == 1},
                 { ConfigValue.WelcomeChannel, async(id) => (ulong)(await serverService.GetServerProfile(id)).WelcomeChannel},
                 { ConfigValue.RuleChannel, async(id) => (ulong)(await serverService.GetServerProfile(id)).RulesChannelId},
                 { ConfigValue.ModRole, async(id) => (ulong)(await serverService.GetModerationData(id)).ModeratorRoleId},
                 { ConfigValue.MutedRole, async(id) => (ulong)(await serverService.GetModerationData(id)).MutedRoleId},
                 { ConfigValue.RuleCount, async(id) => (await moderationService.GetAllRules(id)).Count()},
                 { ConfigValue.DJEnfore, async(id) => (await serverService.GetMusicData(id)).UseDJRoleEnforcement == 1},
                 { ConfigValue.DJRole, async(id) => (ulong)(await serverService.GetMusicData(id)).DJRoleId},
                 { ConfigValue.AllowNSFW, async(id) => (await serverService.GetFunData(id)).AllowNSFW == 1},
                 { ConfigValue.AllowSpamCommand, async(id) => (await serverService.GetFunData(id)).AllowSpamCommand == 1},
                 { ConfigValue.AllowGhostCommand, async(id) => (await serverService.GetFunData(id)).AllowGhostCommand == 1},
                 { ConfigValue.Connect4Channel, async(id) => (ulong)(await serverService.GetGameData(id)).Connect4Channel},
                 { ConfigValue.TicTacToeChannel, async(id) => (ulong)(await serverService.GetGameData(id)).TicTacToeChannel},
                 { ConfigValue.AIChatEnabled, async(id) => (ulong)(await serverService.GetChatData(id)).Enabled == 1},
                 { ConfigValue.AIChatChannel, async(id) => (ulong)(await serverService.GetChatData(id)).AIChatChannelID},
                 { ConfigValue.CustomCommandCount, async(id) => (await moderationService.GetAllCustomCommands(id)).Count()},
                 { ConfigValue.AllowActCommand, async(id) => (await serverService.GetFunData(id)).AllowActCommand == 1}
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
            { ConfigValue.AIChatEnabled,(s) => $"`{(bool)s}`"},
            { ConfigValue.AIChatChannel, (s) => $"{(((ulong)s) == 0 ? "`None`" : $"<#{(ulong)s}>")}"},
            { ConfigValue.CustomCommandCount, (s) => $"`{(int)s}`"},
            { ConfigValue.AllowActCommand, (s) => $"`{(bool)s}`"}
        };

        public async Task<List<DiscordEmbedBuilder>> GetConfigPages(ulong guildId, Permissions perms)
        {
            var valueSets = Enum.GetValues(typeof(ConfigValueSet));
            List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

            foreach(var set in valueSets)
            {
                var casted = (ConfigValueSet)set;
                var permissions = moduleService.ModuleInfo[casted].Permissions;
                bool enabled = (perms & permissions) == permissions;

                var embed = new DiscordEmbedBuilder().WithTitle($"__{(ConfigValueSet)set}__");

                if((ConfigValueSet)set != ConfigValueSet.General)
                    embed.AddField($"• Enabled:", $"`{enabled}`\nWether or not the module(s) is(are) enabled in this server"
                        + $"{(enabled ? "" : $"\n**Enabling**: Give Pepper the {permissions.FormatePermissions()} permission(s)")}");

                if(enabled && configData.ServerConfigValues.ContainsKey(casted))
                    foreach (var value in configData.ServerConfigValues[casted])
                        embed.AddField($"• {value.FieldName}", $"{StringConverions[value.ConfigData].Invoke(await Functions[value.ConfigData].Invoke(guildId))}\n{value.Description}\n**Edit Command(s)**: {value.EditCommand}");

                embeds.Add(embed);
            }

            return embeds;
        }

        public Task GeneratePepperInfoMessage(PepperBot shard, DiscordChannel channel)
        {
            Task.Run(async () =>
            {
                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(GetPepperInfoEmbed(shard.Client.Guilds.Count, shard.Commands.GetCommandCount(), shard.Client.ShardCount, shard.ShardId)
                                .WithFooter("Pepper").WithThumbnail(shard.Client.CurrentUser.AvatarUrl, 30, 30))
                    .AddComponents(
                        new DiscordLinkButtonComponent("https://kunrex.github.io/Pepper.bot/", "website", false, null),
                        new DiscordLinkButtonComponent("https://github.com/kunrex/Pepper.bot", "github (src)", false, null)
                    );

                var message = await messageBuilder.SendAsync(channel);
                var interactivity = shard.Client.GetInteractivity();
            });

            return Task.CompletedTask;
        }

        public Task GeneratePepperInfoMessage(PepperBot shard, DiscordChannel channel, ulong messageId, string userName)
        {
            Task.Run(async () =>
            {
                var messageBuilder = new DiscordMessageBuilder()
                    .AddEmbed(GetPepperInfoEmbed(shard.Client.Guilds.Count, shard.Commands.GetCommandCount(), shard.Client.ShardCount, shard.ShardId)
                                .WithFooter(userName).WithThumbnail(shard.Client.CurrentUser.AvatarUrl, 30, 30))
                    .AddComponents(
                        new DiscordLinkButtonComponent("https://kunrex.github.io/Pepper.bot/", "website", false, null),
                        new DiscordLinkButtonComponent("https://github.com/kunrex/Pepper.bot", "github (src)", false, null)
                    )
                    .WithReply(messageId);

                var message = await messageBuilder.SendAsync(channel);
                var interactivity = shard.Client.GetInteractivity();
            });

            return Task.CompletedTask;
        }

        private DiscordEmbedBuilder GetPepperInfoEmbed(int guildCount, int commandCount, int shardCount, int shardId)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Hi! I'm Pepper",
                Description = $"**About Me:** I'm a girl and I love sleeping and eating.\n I'm in {guildCount} server(s), have a total of {commandCount} commands and have {shardCount} shard(s)."
               + $"The shard ID for this server is {shardId}.",
                Color = DiscordColor.Blurple,
            };

            embed.AddField("__The Modules I offer:__", "** **");
            foreach (var module in moduleService.ModuleNames)
                embed.AddField($"• {module}", "** **", true);

            embed.AddField($"** **", "** **")
                 .AddField("__My Prefix'__", "`pep`, `pepper`", true)
                 .AddField("__My Help Command__", "`pep help`", true)
                 .AddField("__Server Configuration__", "Use the `pep general configuration` command to view my configuration for this server")
                 .AddField("__Enabling and Disabling Modules__", "All the modules (except the general module) can be enabled and disabled depending on the permissions" +
                 " I have. The permissions required for each module is stated in the configuration command and are shown in the help command as well");

            return embed;
        }
    }
}
