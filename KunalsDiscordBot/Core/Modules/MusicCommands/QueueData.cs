using DSharpPlus.Lavalink;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public struct QueueData
    {
        public ulong id { get; set; }
        public string userName { get; set; }
        public LavalinkTrack track { get; set; }
    }
}
