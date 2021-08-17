using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using KunalsDiscordBot.Services.General;
using System.Linq;
using DSharpPlus.CommandsNext;
using KunalsDiscordBot.Core.Attributes.MusicCommands;

namespace KunalsDiscordBot.Services.Music
{
    public class MusicService : IMusicService
    {
        public static Dictionary<ulong, VCPlayer> players = new Dictionary<ulong, VCPlayer>();

        public async Task<string> ClearQueue(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.ClearQueue();
        }

        public async Task<string> ConnnectPlayer(VCPlayer player, DiscordChannel _channel, DiscordChannel _boundChannel) => await player.Connect(_channel, _boundChannel);

        public async Task<string> CreatePlayer(ulong id, LavalinkNodeConnection nodeConnection, LavalinkExtension extension, DiscordChannel _channel, DiscordChannel _boundChannel)
        {
            if (players.ContainsKey(id))
                return "Already joined a voice channel in the server";

            var player = new VCPlayer(id, nodeConnection, extension);
            players.Add(id, player);

            return await ConnnectPlayer(player, _channel, _boundChannel);
        }

        public async Task<string> DisconnectPlayer(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];
            players.Remove(id);

            return await player.Disconnect();
        }

        public async Task<DiscordEmbedBuilder> GetQueue(ulong id)
        {
            if (!players.ContainsKey(id))
                return new DiscordEmbedBuilder
                {
                    Description = "I'm not in a voice Channel?",
                    Color = DiscordColor.Aquamarine
                };

            var player = players[id];

            if (player.connection == null)
                return new DiscordEmbedBuilder
                {
                    Description = "LavaLink not connected",
                    Color = DiscordColor.Aquamarine
                };
            if (player.connection.CurrentState.CurrentTrack == null)
                return new DiscordEmbedBuilder
                {
                    Description = "There are no tracks currently loaded",
                    Color = DiscordColor.Aquamarine
                };

            return await player.GetQueue();
        }

        public async Task<string> Loop(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return $"Loop set to {await player.Loop()}";
        }

        public async Task<string> Move(ulong id, int trackToMove, int newIndex)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Move(trackToMove, newIndex);
        }

        public async Task<DiscordEmbedBuilder> NowPlaying(ulong id)
        {
            if (!players.ContainsKey(id))
                return new DiscordEmbedBuilder
                {
                    Description = "I'm not in a voice Channel?",
                    Color = DiscordColor.Aquamarine
                };

            var player = players[id];

            if (player.connection == null)
                return new DiscordEmbedBuilder
                {
                    Description = "LavaLink not connected",
                    Color = DiscordColor.Aquamarine
                };
            if (player.connection.CurrentState.CurrentTrack == null)
                return new DiscordEmbedBuilder
                {
                    Description = "There are no tracks currently loaded",
                    Color = DiscordColor.Aquamarine
                };

            return await player.NowPlaying();
        }

        public async Task<string> Pause(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Pause();
        }

        public async Task<string> QueueLoop(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return $"Queue Loop set to {await player.QueueLoop()}";
        }

        public async Task<string> Remove(ulong id, int index)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Remove(index);
        }

        public async Task<string> Resume(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Resume();
        }

        public async Task<string> Seek(ulong id, TimeSpan span, bool relative = false)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Seek(span, relative);
        }

        public async Task<string> Play(ulong id, string search, string member, ulong memberId)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";

            return await player.StartPlaying(search, member, memberId);
        }

        public async Task<string> Skip(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            await player.Skip();
            return "Successfully Skipped";
        }

        public async Task<DiscordChannel> GetPlayerChannel(ulong id)
        {
            if (!players.ContainsKey(id))
                return null;

            await Task.CompletedTask;
            return players[id].connection.Channel;
        }

        public async Task<string> Clean(ulong id)
        {
            if (!players.ContainsKey(id))
                return "I'm not in a voice channel?";

            var player = players[id];

            if (player.connection == null)
                return "LavaLink not connected";
            if (player.connection.CurrentState.CurrentTrack == null)
                return "There are no tracks currently loaded";

            return await player.Clean();
        }
    }
}
