using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;

using KunalsDiscordBot.Core.Modules.MusicCommands;

namespace KunalsDiscordBot.Services.Music
{
    public interface IMusicService
    {
        public Task<string> CreatePlayer(ulong id, LavalinkNodeConnection nodeConnection, LavalinkExtension extension, DiscordChannel _channel, DiscordChannel _boundChannel);
        public Task<DiscordChannel> GetPlayerChannel(ulong id);

        public Task<string> ConnnectPlayer(VCPlayer player, DiscordChannel _channel, DiscordChannel _boundChannel);
        public Task<string> DisconnectPlayer(ulong id);

        public Task<string> Play(ulong id, string search, string member, ulong memberId);

        public Task<string> Pause(ulong id);
        public Task<string> Resume(ulong id);

        public Task<string> Remove(ulong id, int index);

        public Task<string> Loop(ulong id);
        public Task<string> QueueLoop(ulong id);

        public Task<DiscordEmbedBuilder> GetQueue(ulong id);
        public Task<DiscordEmbedBuilder> NowPlaying(ulong id);

        public Task<string> Move(ulong id, int trackToMove, int newIndex);
        public Task<string> ClearQueue(ulong id);
        public Task<string> Seek(ulong id, TimeSpan span, bool relative = false);
        public Task<string> Skip(ulong id);
        public Task<string> Clean(ulong id);
    }
}
