//System name spaces
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using DSharpPlus.Lavalink;

using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Music;
using KunalsDiscordBot.Core.Attributes.MusicCommands;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using System.Reflection;
using DSharpPlus.Entities;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Attributes.GeneralCommands;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Modules.Music
{
    [Group("Music")]
    [Decor("Aquamarine", ":musical_note:")]
    [Description("Set of music commands offered by Pepper"), ConfigData(ConfigValueSet.Music)]
    public sealed class MusicCommands : BaseCommandModule
    {
        public static DiscordColor Color = typeof(MusicCommands).GetCustomAttribute<DecorAttribute>().color;

        private readonly IMusicService service;
        private readonly IServerService serverService;

        public MusicCommands(IMusicService _service, IServerService _serverService)
        {
            service = _service;
            serverService = _serverService;
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigPermsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & DSharpPlus.Permissions.Administrator) != DSharpPlus.Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var botVCCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is BotVCNeededAttribute) != null;
            var channel = await service.GetPlayerChannel(ctx.Guild.Id);
            if (botVCCheck && channel == null)
            {
                await ctx.RespondAsync("I'm not in a voice channel?");
                throw new CustomCommandException();
            }

            var userVCCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is UserVCNeededAttribute) != null;
            if(userVCCheck)
            {
                var player = ctx.Member.VoiceState.Channel;
                if (player == null || player.Type != ChannelType.Voice)
                {
                    await ctx.RespondAsync("You need to be in a Voice Channel to run this command");
                    throw new CustomCommandException();
                }
                else if(ctx.Member.VoiceState.IsSelfDeafened || ctx.Member.VoiceState.IsServerDeafened)
                {
                    await ctx.RespondAsync("You can't run this command while deafened");
                    throw new CustomCommandException();
                }
                else if (botVCCheck && player.Id != channel.Id)
                {
                    await ctx.RespondAsync("You need to be in the same Voice Channel as Pepper to run this command");
                    throw new CustomCommandException();
                }
            }

            var DJCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is DJCheckAttribute) != null;
            if(DJCheck && ctx.Member.VoiceState.Channel.Users.ToList().Count > 2)//if one person is in the VC, don't enforce
            {
                var id = (ulong)(await serverService.GetMusicData(ctx.Guild.Id)).DJRoleId;

                if (ctx.Member.Roles.FirstOrDefault(x => x.Id == id) == null)
                {
                    await ctx.RespondAsync(":x: You need the DJ role to run this command");
                    throw new CustomCommandException();
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("ToggleDJ")]
        [Aliases("DJ")]
        [Description("Changes wether or not the DJ role should be enforced for music commands")]
        [CheckConfigPerms, ConfigData(ConfigValue.DJEnfore)]
        public async Task ToggeDJ(CommandContext ctx, bool toChange)
        {
            await serverService.ToggleDJOnly(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce DJ Role` to {toChange}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("DJRole")]
        [Description("Assigns the DJ role for a server")]
        [CheckConfigPerms, ConfigData(ConfigValue.DJRole)]
        public async Task DJRole(CommandContext ctx, DiscordRole role)
        {
            var profile = await serverService.GetMusicData(ctx.Guild.Id).ConfigureAwait(false);
            if (profile.UseDJRoleEnforcement == 0)
            {
                await ctx.RespondAsync("`Enforce DJ Role` must be set to true to use this command, you can do so using the `general ToggleDJ` command").ConfigureAwait(false);
                return;
            }

            await serverService.SetDJRole(ctx.Guild.Id, role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce DJ Role` to {role.Mention}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("Join")]
        [Description("Joins a voice channel")]
        [UserVCNeeded]
        public async Task Join(CommandContext ctx)
        {
            if(await service.GetPlayerChannel(ctx.Guild.Id) != null)
            {
                await ctx.RespondAsync("I'm already in a channel?").ConfigureAwait(false);
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("The LavaLink connection has not been established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();

            var message = await service.CreatePlayer(ctx.Guild.Id, node, lava, channel, ctx.Channel);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Leave")]
        [Description("Leaves the joined channel")]
        [UserVCNeeded, BotVCNeeded, DJCheck]
        public async Task Leave(CommandContext ctx)
        {
            var message = await service.DisconnectPlayer(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Play")]
        [Description("Plays a song")]
        [UserVCNeeded, BotVCNeeded]
        public async Task Play(CommandContext ctx, [RemainingText]string search)
        {           
            var message = await service.Play(ctx.Guild.Id, search, ctx.Member.DisplayName, ctx.Member.Id);

            if(message.Equals("Playing..."))
                await ctx.Channel.SendMessageAsync(await service.NowPlaying(ctx.Guild.Id)).ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Pause")]
        [Description("Pauses the player")]
        [DJCheck, UserVCNeeded,BotVCNeeded]
        public async Task Pause(CommandContext ctx)
        {
            var message = await service.Pause(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Resume")]
        [Description("Resumes the player")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Resume(CommandContext ctx)
        {
            var message = await service.Resume(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Loop")]
        [Description("Toggles if the current track should loop or not")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Loop(CommandContext ctx)
        {
            var message = await service.Loop(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }


        [Command("QueueLoop")]
        [Aliases("ql")]
        [Description("Toggles if the queue should loop or not, this does not include the track being played")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task QueueLoop(CommandContext ctx)
        {
            var message = await service.QueueLoop(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Queue")]
        [Description("Gets the players queue")]
        [BotVCNeeded]
        public async Task GetQueue(CommandContext ctx)
        {
            var embed = await service.GetQueue(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Remove")]
        [Description("Removes a search from the queue")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Remove(CommandContext ctx, int index)
        {
            var message = await service.Remove(ctx.Guild.Id, index);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Skip")]
        [Description("Skips the current track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Skip(CommandContext ctx)
        {
            var message = await service.Skip(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("NowPlaying")]
        [Aliases("np")]
        [Description("Gets info about the current track")]
        [BotVCNeeded]
        public async Task NowPlaying(CommandContext ctx)
        {
            var channel = await service.GetPlayerChannel(ctx.Guild.Id);
            if (channel == null)
            {
                await ctx.Channel.SendMessageAsync("I'm not in a voice channel?");
                return;
            }

            var embed = await service.NowPlaying(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Move")]
        [Description("Moves a track around")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Move(CommandContext ctx, int trackToMove, int newPosition)
        {
            var message = await service.Move(ctx.Guild.Id, trackToMove, newPosition);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            var currentQueue = await service.GetQueue(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(currentQueue).ConfigureAwait(false);
        }

        [Command("Clear")]
        [Description("Clears the track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Clear(CommandContext ctx)
        {
            var message = await service.Skip(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("PlayFrom")]
        [Aliases("pf", "seek")]
        [Description("Starts playing from a specified position")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Seek(CommandContext ctx, TimeSpan span)
        {
            var message = await service.Seek(ctx.Guild.Id, span);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Forward")]
        [Description("Make the track move forward")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Forward(CommandContext ctx, TimeSpan span)
        {
            var message = await service.Seek(ctx.Guild.Id, span, true);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Rewind")]
        [Description("Rewind the track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Rewind(CommandContext ctx, TimeSpan span)
        {
            var message = await service.Seek(ctx.Guild.Id, span.Negate(), true);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Clean")]
        [Description("Cleans the queue. If a track is in the queue and the user who requested is not in the channel, the track is removed")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Clean(CommandContext ctx)
        {
            var message = await service.Clean(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
