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

namespace KunalsDiscordBot.Modules.Music
{
    [Group("Music")]
    [Decor("Aquamarine", ":musical_note:")]
    [Description("Set of music commands offered by Pepper")]
    public sealed class MusicCommands : BaseCommandModule
    {
        private List<VCPlayer> players = new List<VCPlayer>();

        [Command("Join")]
        [Description("Joins a voice channel")]
        public async Task Join(CommandContext ctx)
        {
            if(players.Find(x => x.guildID == ctx.Guild.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("Already joined a voice channel in the server");
                return;
            }

            if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You need to be in a voice channel to use this command.");
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

            if(channel.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("The channel passed is not a voice channel");
                return;
            }

            var player = new VCPlayer(ctx.Guild.Id, node, lava);
            var message = await player.Connect(channel, ctx.Channel);

            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            players.Add(player);
        }

        [Command("Leave")]
        [Description("Leaves the joined channel")]
        public async Task Leave(CommandContext ctx)
        {
            var service = players.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.Channel.SendMessageAsync("Service not found");
                return;
            }

            var player = ctx.Member.VoiceState.Channel;

            if (player == null || player.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("You need to be in a Voice Channel to run this command");
                return;
            }

            var message = await service.Disconnect(player.Name);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            players.Remove(service);//remove the service
        }

        [Command("Play")]
        [Description("Plays a song")]
        public async Task Play(CommandContext ctx, [RemainingText]string search)
        {
            if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("Your are not in a voice channel.");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Channel.Guild.Id);
            if(player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            var message = await player.StartPlaying(search, ctx.Member.Nickname == null ? ctx.Member.Username : ctx.Member.Nickname);

            if(message.Equals("Playing..."))
                await ctx.Channel.SendMessageAsync(await player.NowPlaying()).ConfigureAwait(false);
            else
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Pause")]
        [Description("Pauses the player")]
        public async Task Pause(CommandContext ctx)
        {
            if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if(player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if(player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if(player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Pause();
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Resume")]
        [Description("Resumes the player")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Resume();
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Loop")]
        [Description("Toggles if the current track should loop or not")]
        public async Task Loop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            bool isLooping = await player.Loop();
            await ctx.RespondAsync($"Loop set to {isLooping}.");
        }


        [Command("QueueLoop")]
        [Aliases("ql")]
        [Description("Toggles if the queue should loop or not, this does not include the track being played")]
        public async Task QueueLoop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            bool isLooping = await player.QueueLoop();
            await ctx.RespondAsync($"Queue Loop set to {isLooping}.");
        }

        [Command("Queue")]
        [Description("Gets the players queue")]
        public async Task GetQueue(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var embed = await player.GetQueue();
            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("Remove")]
        [Description("Removes a search from the queue")]
        public async Task Remove(CommandContext ctx, int index)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var result = await player.Remove(index);
            await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }

        [Command("Skip")]
        [Description("Skips the current track")]
        public async Task Skip(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            await ctx.Channel.SendMessageAsync("Skipped").ConfigureAwait(false);
            await player.Skip();
        }

        [Command("NowPlaying")]
        [Aliases("np")]
        [Description("Gets info about the current track")]
        public async Task NowPlaying(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var embed = await player.NowPlaying();
            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        [Command("move")]
        [Description("Moves a track around")]
        public async Task Move(CommandContext ctx, int trackToMove, int newPosition)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Move(trackToMove, newPosition);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

            var currentQueue = await player.GetQueue();
            await ctx.Channel.SendMessageAsync(currentQueue).ConfigureAwait(false);
        }

        [Command("clear")]
        [Description("Clears the track")]
        public async Task Clear(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.ClearQueue();
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("playfrom")]
        [Aliases("pf", "seek")]
        [Description("Starts playing from a specified position")]
        public async Task Seek(CommandContext ctx, TimeSpan span)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Seek(span);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("forward")]
        [Description("move a few seconds forward")]
        public async Task Forward(CommandContext ctx, TimeSpan span)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Seek(span, true);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("rewind")]
        [Description("move a few seconds backward")]
        public async Task Rewind(CommandContext ctx, TimeSpan span)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel");
                return;
            }

            var player = players.Find(x => x.guildID == ctx.Guild.Id);

            if (player == null)
            {
                await ctx.Channel.SendMessageAsync("Pepper isn't in a voice channel");
                return;
            }

            if (player.connection == null)
            {
                await ctx.Channel.SendMessageAsync("LavaLink not connected");
                return;
            }

            if (player.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("There are no tracks currently loaded");
                return;
            }

            var message = await player.Seek(-span, true);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
