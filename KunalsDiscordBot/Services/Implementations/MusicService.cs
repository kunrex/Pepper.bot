using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.MusicCommands;
using DiscordBotDataBase.Dal.Models.Servers.Models.Music;

namespace KunalsDiscordBot.Services.Music
{
    public class MusicService : IMusicService
    {
        public Dictionary<ulong, VCPlayer> Players { get; private set; } = new Dictionary<ulong, VCPlayer>();
        public MusicModuleData ModuleData { get; }

        public MusicService(PepperConfigurationManager configurationManager, IModuleService moduleService)
        {
            ModuleData = configurationManager.MusicConfig;

            ModuleData.Color = moduleService.ModuleInfo[ConfigValueSet.Music].Color;
        }

        public async Task<string> ClearQueue(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.ClearQueue();
        }

        public async Task<string> ConnnectPlayer(VCPlayer player, DiscordChannel _channel, DiscordChannel _boundChannel) => await player.Connect(_channel, _boundChannel);

        public async Task<string> CreatePlayer(ulong id, LavalinkNodeConnection nodeConnection, LavalinkExtension extension, DiscordChannel _channel, DiscordChannel _boundChannel)
        {
            var player = new VCPlayer(ModuleData, nodeConnection, extension);

            Players.Add(id, player);
            player.OnDisconnect.WithEvent(() => Players.Remove(id));

            return await ConnnectPlayer(player, _channel, _boundChannel);
        }

        public async Task<string> DisconnectPlayer(ulong id) => await Players[id].Disconnect();

        public async Task<List<DiscordEmbedBuilder>> GetQueue(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return new List<DiscordEmbedBuilder>()
                {
                    new DiscordEmbedBuilder().WithDescription("There are no tracks currently loaded")
                };

            return await player.GetQueue();
        }

        public async Task<string> Loop(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return $"Loop set to {await player.Loop()}";
        }

        public async Task<string> Move(ulong id, int trackToMove, int newIndex)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Move(trackToMove, newIndex);
        }

        public async Task<DiscordEmbedBuilder> NowPlaying(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return new DiscordEmbedBuilder().WithDescription("There are no tracks currently loaded");

            return await player.NowPlaying();
        }

        public async Task<string> Pause(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Pause();
        }

        public async Task<string> QueueLoop(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return $"Queue Loop set to {await player.QueueLoop()}";
        }

        public async Task<string> Remove(ulong id, int index)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Remove(index);
        }

        public async Task<string> Resume(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Resume();
        }

        public async Task<string> Seek(ulong id, TimeSpan span, bool relative = false)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Seek(span, relative);
        }

        public async Task<DiscordEmbedBuilder> Play(ulong id, string search, string member, ulong memberId) => await Players[id].StartPlaying(search, member, memberId);

        public async Task<DiscordEmbedBuilder> Play(ulong id, string member, ulong memberId, PlaylistTrack[] tracks)
        {
            var player = Players[id];
            var subset = tracks.Take(ModuleData.MaxQueueLength - player.Queue.Count).ToArray();

            (bool completed, int index) = await player.QueuePlaylist(subset, member, memberId);
            if (!completed)
                return new DiscordEmbedBuilder()
                    .WithColor(ModuleData.Color)
                    .WithDescription($"Failed to queue full playlist, track search failed for track at index {index + 1}");
            else
                return new DiscordEmbedBuilder()
                    .WithColor(ModuleData.Color)
                    .WithDescription($"Queued playlist with {subset.Length} tracks");
        }

        public async Task<string> Skip(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            await player.Skip();
            return "Successfully Skipped";
        }

        public Task<DiscordChannel> GetPlayerChannel(ulong id)
        {
            if(!Players.ContainsKey(id))
                return Task.FromResult<DiscordChannel>(null);

            return Task.FromResult(Players[id].Connection.Channel);
        }

        public async Task<string> Clean(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Clean();
        }

        public async Task<string> Shuffle(ulong id)
        {
            var player = Players[id];

            if (player.Connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Shuffle();
        }
    }
}
