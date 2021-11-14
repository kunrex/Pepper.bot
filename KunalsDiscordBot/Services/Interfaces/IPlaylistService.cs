using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal.Models.Servers.Models.Music;

namespace KunalsDiscordBot.Services.Music
{
    public interface IPlaylistService
    {
        public Task<bool> CreatePlaylist(ulong id, ulong authorId, string name, string[] tracks);
        public Task<bool> DeletePlaylist(ulong id, string name);

        public Task<Playlist> GetPlaylist(ulong id, string name);
        public Task<IEnumerable<Playlist>> GetPlaylists(ulong id);

        public Task<IEnumerable<PlaylistTrack>> GetTracks(Playlist playlist);
        public Task<IEnumerable<PlaylistTrack>> GetTracks(ulong id, string playlistName);

        public Task<bool> AddTrack(Playlist playlist, ulong authorId, string track);
        public Task<bool> AddTrack(ulong id, string name, ulong authorId, string track);

        public Task<bool> RemoveTrack(Playlist playlist, int index);
        public Task<bool> RemoveTrack(ulong id, string name, int index);
    }
}
