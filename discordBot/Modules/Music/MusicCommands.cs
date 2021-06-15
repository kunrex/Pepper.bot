//System name spaces
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
    public class MusicCommands : BaseCommandModule
    {
        private List<MusicService> musicServices = new List<MusicService>();

        [Command("Join")]
        [Description("Joins a voice channel")]
        public async Task Join(CommandContext ctx)
        {
            if(musicServices.Find(x => x.guildID == ctx.Guild.Id) != null)
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

            var musicService = new MusicService(ctx.Guild.Id, node, lava);
            await musicService.Connect(ctx, channel, ctx.Channel);

            musicServices.Add(musicService);
        }

        [Command("Leave")]
        [Description("Leaves the joined channel")]
        public async Task Leave(CommandContext ctx)
        {
            var channel = ctx.Member.VoiceState.Channel;

            if (channel == null || channel.Type != ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("You need to be in a Voice Channel to run this command");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if(service == null)
            {
                await ctx.Channel.SendMessageAsync("Service not found");
                return;
            }

            await service.Disconnect(ctx, channel.Name);

            musicServices.Remove(service);//remove the service
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

            var service = musicServices.Find(x => x.guildID == ctx.Channel.Guild.Id);
            if(service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            await service.StartPlaying(search, ctx);
        }

        [Command("Pause")]
        [Description("Pauses the player")]
        public async Task Pause(CommandContext ctx)
        {
            if(ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if(service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if(service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if(service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            await service.Pause(ctx);
        }

        [Command("Resume")]
        [Description("Resumes the player")]
        public async Task Resume(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            await service.Resume(ctx);
        }

        [Command("Loop")]
        [Description("Toggles if the current track should loop or not")]
        public async Task Loop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            bool isLooping = await service.Loop();
            await ctx.RespondAsync($"Loop set to {isLooping}.");
        }


        [Command("QueueLoop")]
        [Aliases("ql")]
        [Description("Toggles if the queue should loop or not, this does not include the track being played")]
        public async Task QueueLoop(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            bool isLooping = await service.QueueLoop();
            await ctx.RespondAsync($"Queue Loop set to {isLooping}.");
        }

        [Command("Queue")]
        [Description("Gets the players queue")]
        public async Task GetQueue(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var embed = await service.GetQueue();
            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("Remove")]
        [Description("Removes a search from the queue")]
        public async Task Remove(CommandContext ctx, int index)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var result = await service.Remove(index);
            await ctx.Channel.SendMessageAsync(result).ConfigureAwait(false);
        }

        [Command("Skip")]
        [Description("Skips the current track")]
        public async Task Skip(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            await service.Skip(ctx);
        }

        [Command("NowPlaying")]
        [Aliases("np")]
        [Description("Gets info about the current track")]
        public async Task NowPlaying(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel");
                return;
            }

            var service = musicServices.Find(x => x.guildID == ctx.Guild.Id);

            if (service == null)
            {
                await ctx.RespondAsync("Pepper isn't in a voice channel");
                return;
            }

            if (service.connection == null)
            {
                await ctx.RespondAsync("LavaLink not connected");
                return;
            }

            if (service.connection.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks currently loaded");
                return;
            }

            var embed = await service.NowPlaying();
            await ctx.Channel.SendMessageAsync(embed: embed);
        }
    }
}
