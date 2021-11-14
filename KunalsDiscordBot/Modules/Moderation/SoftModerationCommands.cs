using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.DiscordModels;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.Attributes.ModerationCommands;

namespace KunalsDiscordBot.Modules.Moderation.SoftModeration
{
    [Group("SoftModeration")]
    [Aliases("softmod", "sm"), Decor("Blurple", ":scales:")]
    [Description("Soft Moderation commands."), ModuleLifespan(ModuleLifespan.Transient)]
    [RequireBotPermissions(Permissions.Administrator), ConfigData(ConfigValueSet.Moderation)]
    public class SoftModerationCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IModerationService modService;
        private readonly IServerService serverService;

        public SoftModerationCommands(IModerationService moderationService, IServerService _serverService, IModuleService moduleService)
        {
            modService = moderationService;
            serverService = _serverService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Moderation];
        }

        private static readonly int ThumbnailSize = 20;

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var checkMute = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckMuteRoleAttribute) != null;
            if(checkMute)
            {
                var profile = await serverService.GetModerationData(ctx.Guild.Id);
                var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

                if (role == null)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = $"There is no muted role stored for server: {ctx.Guild.Name}. Use the `soft moderation setmuterole` command to do so",
                        Color = ModuleInfo.Color
                    }.WithFooter($"Moderator: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }

                var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
                if(botMember.GetHighest() < role.Position)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = $"The mute role {role.Mention}, is higher than my higher role. Thus I cannot add or remove it.",
                        Color = ModuleInfo.Color
                    }.WithFooter($"Moderator: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("SetModRole")]
        [Description("Assigns the moderator role for the server. This command can only be ran by an administrator")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.ModRole)]
        public async Task SetModRole(CommandContext ctx, DiscordRole role)
        {
            await serverService.ModifyData(await serverService.GetModerationData(ctx.Guild.Id), x => x.ModeratorRoleId = (long)role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {role.Mention} as the moderator role for the server",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("SetMuteRole")]
        [Description("Sets the mute role of a server")]
        [RequireUserPermissions(Permissions.Administrator), ConfigData(ConfigValue.MutedRole)]
        public async Task SetMuteRole(CommandContext ctx, DiscordRole role)
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
            if(botMember.GetHighest() < role.Position)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"{role.Mention} as it is higher than my highest role in this server. I will not be able to add or remove it. Please assign a role lower than my highest role or give me a higher role",
                    Color = ModuleInfo.Color
                }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
                return;
            }

            await serverService.ModifyData(await serverService.GetModerationData(ctx.Guild.Id), x => x.MutedRoleId = (long)role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Mute Role Saved",
                Description = $"Succesfully stored {role.Mention} as the mute role for the server",
                Color = ModuleInfo.Color
            }.WithFooter($"Admin: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("ChangeNickName")]
        [Aliases("cnn")]
        [RequireBotPermissions(Permissions.ManageNicknames), ModeratorNeeded]
        public async Task ChangeNickName(CommandContext ctx, DiscordMember member, [RemainingText] string newNick)
        {
            try
            {
                await member.ModifyAsync((MemberEditModel obj) => obj.Nickname = newNick);

                await ctx.Channel.SendMessageAsync($"Changed nickname for {member.Username} to {newNick}");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Failed to change nickname for {member.Mention}").ConfigureAwait(false);
            }      
        }

        [Command("VCMute")]
        [Aliases("vcm"), ModeratorNeeded]
        public async Task VoiceMuteMember(CommandContext ctx, DiscordMember member, bool toMute)
        {
            try
            {
                await member.SetMuteAsync(toMute).ConfigureAwait(false);

                await ctx.RespondAsync($"{(toMute ? "VC unmuted" : "VC muted")} {member.Username} in the voice channels");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Failed to {(toMute ? "VC unmute" : "VC mute")} the spcified user.").ConfigureAwait(false);
            }
        }

        [Command("VCDeafen")]
        [Aliases("vcd"), ModeratorNeeded]
        public async Task VoiceDeafenMember(CommandContext ctx, DiscordMember member, bool toDeafen)
        {
            try
            {
                await member.SetDeafAsync(toDeafen).ConfigureAwait(false);

                await ctx.RespondAsync($"{(toDeafen ? "VC undeafened" : "VC deafened")} {member.Username} in the voice channels");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync($"Failed to {(toDeafen ? "VC undeafen" : "VC deafen")} the spcified user.").ConfigureAwait(false);
            }
        }

        [Command("AddInfraction")]
        [Aliases("Infract"), Description("Adds an infraction for a user"), ModeratorNeeded]
        public async Task AddInfraction(CommandContext ctx, DiscordMember member, [RemainingText]  string reason = "Unpsecified")
        {
            var id = await modService.AddInfraction(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Infraction Added [Id: {id}]",
                Description = $"Reason: {reason}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("AddEndorsement")]
        [Aliases("Endorse"), Description("Adds an endorsement for a user"), ModeratorNeeded]
        public async Task AddEndorsement(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unpsecified")
        {
            var id = await modService.AddEndorsement(member.Id, ctx.Guild.Id, ctx.Member.Id, reason);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Endrosement Added [Id: {id}]",
                Description = $"Reason: {reason}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Moderator: {ctx.Member.DisplayName} #{ctx.Member.Discriminator}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)).ConfigureAwait(false);
        }

        [Command("GetEndorsement")]
        [Aliases("ge"), Description("Gets an infraction using its ID")]
        public async Task GetEndorsement(CommandContext ctx, int endorsementID)
        {
            var endorsement = await modService.GetEndorsement(endorsementID);

            if(endorsement == null)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist");
                return;
            }

            if((ulong)endorsement.GuildID != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Endorsement with this Id doesn't exist exist in this server");
                return;
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Endorsement {endorsement.Id}",
                Description = $"User: <@{(ulong)endorsement.UserId}>\nReason: {endorsement.Reason}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)endorsement.ModeratorID).ConfigureAwait(false)).DisplayName}")).ConfigureAwait(false);
        }

        [Command("GetInfraction")]
        [Aliases("gi"), Description("Gets an infraction using its ID")]
        public async Task GetInfraction(CommandContext ctx, int infractionID)
        {
            var infraction = await modService.GetInfraction(infractionID);

            if(infraction == null)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist");
                return;
            }

            if ((ulong)infraction.GuildID != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Infraction with this Id doesn't exist exist in this server");
                return;
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Infraction {infraction.Id}",
                Description = $"User: <@{(ulong)infraction.UserId}>\nReason: {infraction.Reason}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)infraction.ModeratorID).ConfigureAwait(false)).DisplayName}")).ConfigureAwait(false);
        }

        [Command("Rule")]
        [Description("Displays a rule by its index")]
        public async Task AddRule(CommandContext ctx, int index)
        {
           var rule =  await modService.GetRule(ctx.Guild.Id, index - 1).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Rule {index}",
                Description = $"{(rule == null ? "Rule doesn't exist" : rule.RuleContent)}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}")).ConfigureAwait(false);
        }

        [Command("ClearChat")]
        [Aliases("Clear"), Description("Deletes `x` number of messages")]
        [RequireBotPermissions(Permissions.ManageMessages), ModeratorNeeded]
        public async Task ClearChat(CommandContext ctx, int number)
        {
            await ctx.Channel.DeleteMessagesAsync(await ctx.Channel.GetMessagesAsync(number));

            var message = await ctx.Channel.SendMessageAsync("**Chat has been cleaned**").ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(2));

            await message.DeleteAsync();
        }

        [Command("Slowmode")]
        [Aliases("slow"), Description("Sets the slow mode for a channel")]
        [RequireBotPermissions(Permissions.ManageChannels), ModeratorNeeded]
        public async Task SlowMode(CommandContext ctx, int seconds)
        {
            if(ctx.Channel.PerUserRateLimit == seconds)
            {
                await ctx.RespondAsync($"Slow mode for {ctx.Channel.Mention} already is {seconds} seconds?").ConfigureAwait(false);
                return;
            }

            await ctx.Channel.ModifyAsync((ChannelEditModel obj) => obj.PerUserRateLimit = seconds);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = $"{(seconds == 0 ? $"Disabled slow mode for {ctx.Channel.Mention}": $"Set Slow Mode for {ctx.Channel.Mention} to {seconds} seconds")}",
                Color = ModuleInfo.Color
            }.WithFooter($"Moderator: {ctx.Member.DisplayName}")).ConfigureAwait(false);
        }

        [Command("SetNSFW")]
        [Description("Changes the NSFW status of a channel")]
        [RequireBotPermissions(Permissions.ManageChannels), ModeratorNeeded]
        public async Task NSFW(CommandContext ctx, bool toSet)
        {
            if(ctx.Channel.IsNSFW == toSet)
            {
                await ctx.RespondAsync($"{(toSet ? "Channel already is NSFW?" : "Channel isn't NSFW in the first place?")}");
                return;
            }

            await ctx.Channel.ModifyAsync((ChannelEditModel obj) => obj.Nsfw = toSet);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Description = $"{ctx.Channel.Mention} {(toSet ? "is now NSFW" : "is not NSFW anymore")}",
                Color = ModuleInfo.Color
            }.WithFooter($"Moderator: {ctx.Member.DisplayName}")).ConfigureAwait(false);
        }

        [Command("AddEmoji")]
        [Description("Addes an emoji to the server")]
        [RequireBotPermissions(Permissions.ManageEmojis), ModeratorNeeded]
        public async Task AddEmoji(CommandContext ctx, string name, string url)
        {
            if(ctx.Guild.Emojis.Values.FirstOrDefault(x => x.Name.ToLower() == name) != null)
            {
                await ctx.RespondAsync("emoji with this name already exists").ConfigureAwait(false);
                return;
            }

            if (ctx.Guild.Emojis.Count == 50 + (((int)ctx.Guild.PremiumTier) * 50))
            {
                await ctx.RespondAsync($"Emoji cap reached ({((int)ctx.Guild.PremiumTier) * 50})");
                return;
            }

            await ctx.Channel.SendMessageAsync("This might take a second").ConfigureAwait(false);
            using (WebClient webClient = new WebClient())
            {
                byte[] data = webClient.DownloadData(url);

                using (MemoryStream stream = new MemoryStream(data))
                {
                    stream.Position = 0;
                    var emoji = await ctx.Guild.CreateEmojiAsync(name, stream);

                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = $"Emoji Added! <:{emoji.Name}:{emoji.Id}>",
                        Color = ModuleInfo.Color
                    }.WithFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}")).ConfigureAwait(false);
                }
            }       
        }

        [Command("RemoveEmoji")]
        [Description("Removes an emoji from the server")]
        [ModeratorNeeded]
        [RequireBotPermissions(Permissions.ManageEmojis)]
        public async Task RemoveEmoji(CommandContext ctx, string name)
        {
            await ctx.Channel.SendMessageAsync("This might take a second").ConfigureAwait(false);

            var emoji = ctx.Guild.Emojis.Values.FirstOrDefault(x => x.Name == name);
            if (emoji == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "Emoji not found",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);

                return;
            }

            await ctx.Guild.DeleteEmojiAsync(await ctx.Guild.GetEmojiAsync(emoji.Id));

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = $"Removed Emoji {name}",
                Color = ModuleInfo.Color
            }.WithFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("Mute")]
        [Description("Mutes a member")]
        [ModeratorNeeded, CheckMuteRole]
        public async Task Mute(CommandContext ctx, DiscordMember member, TimeSpan span, [RemainingText] string reason = "Unspecified")
        {
            var profile = await serverService.GetModerationData(ctx.Guild.Id);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Member is already muted");
                return;
            }

            await member.GrantRoleAsync(role).ConfigureAwait(false);
            int id = await modService.AddMute(member.Id, ctx.Guild.Id, ctx.Member.Id, reason, span.ToString());
            BotEventFactory.CreateScheduledEvent().WithSpan(span).WithEvent((s, e) =>
            {
                if (member.Roles.FirstOrDefault(x => x.Id == role.Id) != null)
                    Task.Run(async () => await member.RevokeRoleAsync(role).ConfigureAwait(false));
            }).Execute();

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Muted Member {member.DisplayName}",
                Color = ModuleInfo.Color,
            }.AddField("Reason: ", reason)
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize)
             .WithFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Unmute")]
        [Description("Unmutes a member")]
        [ModeratorNeeded, CheckMuteRole]
        public async Task UnMute(CommandContext ctx, DiscordMember member, [RemainingText] string reason = "Unspecified")
        {
            var profile = await serverService.GetModerationData(ctx.Guild.Id);
            var role = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Id == (ulong)profile.MutedRoleId).Value;

            if (member.Roles.FirstOrDefault(x => x.Id == role.Id) == null)
            {
                await ctx.Channel.SendMessageAsync("Member isn't muted?");
                return;
            }

            await member.RevokeRoleAsync(role).ConfigureAwait(false);

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Unmuted Member {member.DisplayName}",
                Color = ModuleInfo.Color,
            }.AddField("Reason: ", reason)
             .WithFooter($"Moderator: {ctx.Member.DisplayName} at {DateTime.Now}")
             .WithThumbnail(member.AvatarUrl, ThumbnailSize, ThumbnailSize);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("GetMute")]
        [Description("Gets a mute event using its ID")]
        public async Task GetMute(CommandContext ctx, int muteId)
        {
            var mute = await modService.GetMute(muteId);

            if (mute == null)
            {
                await ctx.Channel.SendMessageAsync("Mute with this Id doesn't exist");
                return;
            }

            if ((ulong)mute.GuildID != ctx.Guild.Id)
            {
                await ctx.Channel.SendMessageAsync("Mute with this Id doesn't exist exist in this server");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Mute {mute.Id}",
                Description = $"User: <@{(ulong)mute.UserId}>\nReason: {mute.Reason}",
                Color = ModuleInfo.Color,
            }.WithFooter($"Moderator: {(await ctx.Guild.GetMemberAsync((ulong)mute.ModeratorID).ConfigureAwait(false)).DisplayName}");

            var span = TimeSpan.Parse(mute.Time);
            embed.AddField("Time: ", $"{span.Days} days, {span.Hours} hours, {span.Minutes} minutes, {span.Seconds} seconds");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("CustomCommandsList")]
        [Description("Shows all custom commands in a server"), Aliases("cclist")]
        public async Task CustomCommandsList(CommandContext ctx)
        {
            var customCommands = await modService.GetAllCustomCommands(ctx.Guild.Id);
            if (!customCommands.Any())
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "This server has no custom commands",
                    Color = ModuleInfo.Color,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User: {ctx.Member.DisplayName}, at {DateTime.Now}" }
                });

                return;
            }

            var pages = ctx.Client.GetInteractivity().GetPages(customCommands, x => (x.CommandName, "** **"), new EmbedSkeleton
            {
                Color = ModuleInfo.Color,
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName }
            }).ToArray();

            if (pages.Length == 1)
                await ctx.RespondAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable);
        }

        [GroupCommand, Hidden]
        public async Task CustomCommand(CommandContext ctx, string name)
        {
            name = name.ToLower();

            var customCommand = await modService.GetCustomCommand(ctx.Guild.Id, name);
            if (customCommand == null)
                throw new CommandNotFoundException($"moderation {name}");

            await ctx.RespondAsync(customCommand.CommandContent);
        }

        [Command("FilteredWordList")]
        [Description("Shows all filtered words in a server"), ModeratorNeeded, Aliases("wordslist")]
        public async Task FilteredWordList(CommandContext ctx)
        {
            var words = await modService.GetAllFilteredWords(ctx.Guild.Id);
            if (!words.Any())
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "This server has no filtered words",
                    Color = ModuleInfo.Color,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"User: {ctx.Member.DisplayName}, at {DateTime.Now}" }
                });

                return;
            }

            var pages = ctx.Client.GetInteractivity().GetPages(words, x => ("", $"||{x.Word}||"), new EmbedSkeleton
            {
                Color = ModuleInfo.Color,
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName }
            }).ToArray();

            if (pages.Length == 1)
                await ctx.RespondAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable);
        }
    }
}
