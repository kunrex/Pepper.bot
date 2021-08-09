using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DiscordBotDataBase.Dal.Models.Servers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Services
{
    public abstract class BotService
    {
        public static readonly IReadOnlyList<ConfigDataSet> Configdata = GetConfigData();

        private static List<ConfigDataSet> GetConfigData()
        {
            var modules = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule))).ToList();
            List<ConfigDataSet> dataSet = new List<ConfigDataSet>();

            foreach (var module in modules)
            {
                var name = module.GetCustomAttribute<GroupAttribute>().Name;

                foreach (var command in module.GetMethods())
                {
                    var atrribute = command.GetCustomAttribute<ConfigDataAttribute>();
                    if (atrribute == null)
                        continue;

                    var found = false;
                    var commandName = $"{name} {command.GetCustomAttribute<CommandAttribute>().Name}";
                    for (int i = 0; i < dataSet.Count; i++)
                        if (dataSet[i].data == atrribute.data)
                        {
                            dataSet[i] = new ConfigDataSet { data = dataSet[i].data, editCommand = dataSet[i].editCommand + $", `{commandName}`" };
                            found = true;
                        }

                    if (!found)
                        dataSet.Add(new ConfigDataSet { data = atrribute.data, editCommand = $"`pep {commandName}`" });
                }
            }

            return dataSet;
        }

        private static string GetEditCommand(ConfigData data) => Configdata.FirstOrDefault(x => x.data == data).editCommand.ToLower();

        public static DiscordEmbedBuilder GetBotInfo(DiscordClient client, DiscordMember member, int thumbnailSize)
        {
            string modules = string.Empty;
            var allModules = Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)));

            var embed = new DiscordEmbedBuilder
            {
                Title = "Hi! I'm Pepper",
                Description = $"**About Me:** I'm a girl and I love sleeping and eating.\n I'm in {client.Guilds.Count} server(s) and have {client.ShardCount} shard(s)." +
                $"The shard ID for this server is {client.ShardId}.",
                Color = DiscordColor.Blurple,
                Footer = GetEmbedFooter(member == null ? "Pepper" : $"User: {member.DisplayName} #{member.Discriminator}"),
                Thumbnail = GetEmbedThumbnail(client.CurrentUser, thumbnailSize)
            }.AddField("__The Modules I offer:__", "** **");

            foreach (var type in allModules)
                embed.AddField($"• {type.GetCustomAttribute<GroupAttribute>().Name}", "** **", true);

            embed.AddField($"** **", "** **")
                 .AddField("__My Prefix'__", "`pep`, `pepper`", true)
                 .AddField("__My Help Command__", "pep help", true)
                 .AddField("__Contribute__", "The githib repo isn't public yet", true)
                 .AddField("__Configuration__", "use the `pep general configuration` command to view and edit my configuration for this server", true)
                 .AddField("__Moderation and Solf Moderation__", "I also offer commands for server moderation, If you can't see them in the help command" +
                 ", its probably because I haven't been given the `Administrator` permission");

            return embed;
        }

        public static DiscordEmbedBuilder GetLeaveEmbed() => new DiscordEmbedBuilder
        {
            Title = "Had a great time here ppl!",
            Description = "Note: The config for this server will be deleted, in case I'm ever readded it would be a fresh new one",
            Footer = GetEmbedFooter($"Left server at {DateTime.Now}")
        };

        public static DiscordEmbedBuilder.EmbedThumbnail GetEmbedThumbnail(DiscordUser user, int thumbnailSize) => new DiscordEmbedBuilder.EmbedThumbnail
        {
            Url = user.AvatarUrl,
            Height = thumbnailSize,
            Width = thumbnailSize
        };

        public static DiscordEmbedBuilder.EmbedFooter GetEmbedFooter(string text) => new DiscordEmbedBuilder.EmbedFooter
        {
            Text = text
        };

        public static DiscordEmbedBuilder GetGeneralConfig(ServerProfile profile)
        {
            return new DiscordEmbedBuilder
            {
                Title = "__General__",
            }.AddField($"• Restrict Config Perms: `{profile.RestrictPermissionsToAdmin == 1}`", "When set to true only admins can edit the configuration"
              + $"\n**Edit Command**: {GetEditCommand(ConfigData.EnforcePermissions)}")
             .AddField($"• Log Errors: `{profile.LogErrors == 1}`", "When set to true, a message is sent if an error happens during command execution"
              + $"\n**Edit Command**: {GetEditCommand(ConfigData.LogErrors)}")
             .AddField($"• Log New Members: `{profile.LogNewMembers == 1}`", "When set to true, a message is sent if a new member joins or or a member leaves the server"
              + $"\n**Edit Command**: {GetEditCommand(ConfigData.LogMembers)}")
             .AddField($"• Log Channel:", $" {(profile.LogChannel == 0 ? "`None`" : $"<#{(ulong)profile.LogChannel}>")}\nThe log channel of the server, or the channel in which welcome messages are sent. If null this defaults to the general channel of the server"
             + $"\n**Edit Command**: {GetEditCommand(ConfigData.LogChannel)}");
        }

        public static DiscordEmbedBuilder GetModConfig(ServerProfile profile, bool hasMod, int ruleCount)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "__Moderation and Soft Moderation__",
            }.AddField($"• Enabled: `{hasMod}`", "Wether or not the moderation and soft moderation modules are enabled in thi server"
                + $"{(hasMod ? "" : "\n**Enabling**: Give Pepper the `Administrator` permission")}");

            if (hasMod)
            {
                embed.AddField($"• Muted Role:", $"{(profile.MutedRoleId == 0 ? "`None`" : $"<@&{(ulong)profile.MutedRoleId}>")}\nThe role that ias assigned when a member is muted"
                     + $"\n**Edit Command**: `{GetEditCommand(ConfigData.MutedRole)}", true);
                embed.AddField($"• Rule Count: `{ruleCount}`", "The amount of rules in the server"
                     + $"\n**Edit Commands**: {GetEditCommand(ConfigData.RuleCount)}", true);
                embed.AddField($"• Rule Channel:", $"{(profile.RulesChannelId == 0 ? "`None`" : $"<#{(ulong)profile.RulesChannelId}>")}\nThe rule channel of the server (if any)"
                     + $"\n**Edit Command**: {GetEditCommand(ConfigData.RuleChannel)}");
                embed.AddField($"• Moderator Role:", $"{(profile.ModeratorRoleId == 0 ? "`None`" : $" <@&{(ulong)profile.ModeratorRoleId}>")}\nThe moderator role of this server"
                     + $"\n**Edit Command**: {GetEditCommand(ConfigData.ModRole)}", true);
            }

            return embed;
        }

        public static DiscordEmbedBuilder GetMusicAndFunConfig(ServerProfile profile)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "__Music and Fun Commands__",
            }.AddField("__Music__", "** **")
             .AddField($"• Enforce DJ Permissions: `{(profile.UseDJRoleEnforcement == 1)}`", "When set to true, a member cannot run most music commands without the DJ role"
              + $"\n**Edit Command**: {GetEditCommand(ConfigData.DJEnfore)}")
             .AddField($"• DJ Role:", $"{(profile.DJRoleId == 0 ? "`None`" : $" <@&{(ulong)profile.DJRoleId}>")}. \nThe DJ role for this server"
              + $"\n**Edit Command**: {GetEditCommand(ConfigData.DJRole)}", true);

            embed.AddField("__Fun__", "** **")
                 .AddField($"• Allow NSFW: `{profile.AllowNSFW == 1}`", "When set to true the bot can post NSFW posts from NSFW subreddits"
                 + $"\n**Edit Command**: {GetEditCommand(ConfigData.AllowNSFW)}"); 

            return embed;
        }
    }
}
