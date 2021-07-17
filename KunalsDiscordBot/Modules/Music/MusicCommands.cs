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

namespace KunalsDiscordBot.Modules.Music
{
    [Group("Music")]
    [DecorAttribute("Aquamarine", ":musical_note:")]
    [Description("Set of music commands offered by Pepper")]
    public sealed class MusicCommands : BaseCommandModule
    {
        private readonly IMusicService service;

        public MusicCommands(IMusicService _service) => service = _service;

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
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

            var DJCheck = await service.CheckDJRole(ctx);
            if(!DJCheck && ctx.Member.VoiceState.Channel.Users.ToList().Count > 2)//if one person is in the VC, don't enforce
            {
                await ctx.RespondAsync(":x: You need the DJ role to run this command");
                throw new CustomCommandException();
            }

            await base.BeforeExecutionAsync(ctx);
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
            var message = await service.Play(ctx.Guild.Id, search, ctx.Member.DisplayName);

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
    }
}
