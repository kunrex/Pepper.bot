using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal;
using KunalsDiscordBot.Services.General;
using DiscordBotDataBase.Dal.Models.Servers.Models.Music;

namespace KunalsDiscordBot.Services.Music
{
    public class PlaylistService : DatabaseService, IPlaylistService
    {
        private readonly IServerService serverService;

        public PlaylistService(DataContext _context, IServerService _serverService) : base(_context)
        {
            serverService = _serverService;
        }

        public async Task<bool> CreatePlaylist(ulong id, ulong authorId, string name)
        {
            var musicData = await serverService.GetMusicData(id);

            var playlist = new Playlist
            {
                AuthorId = (long)authorId,
                PlaylistName = name,
                Tracks = new List<PlaylistTrack>()
            };

            musicData.Playlists.Add(playlist);
            return await UpdateEntity(musicData);
        }

        public async Task<bool> DeletePlaylist(ulong id, string name)
        {
            var playlist = await GetPlaylist(id, name);
            if (playlist == null)
                return false;

            return await RemoveEntity(playlist);
        }

        public async Task<bool> RenamePlaylist(ulong id, string name, string newName)
        {
            var playlist = await GetPlaylist(id, name);
            if (playlist == null)
                return false;

            playlist.PlaylistName = newName;
            return await UpdateEntity(playlist);
        }

        public async Task<Playlist> GetPlaylist(ulong id, string name) => (await GetPlaylists(id)).FirstOrDefault(x => x.PlaylistName == name);

        public Task<IEnumerable<Playlist>> GetPlaylists(ulong id)
        {
            var casted = (long)id;
            return Task.FromResult(context.ServerPlaylists.AsEnumerable().Where(x => x.MusicDataId == casted));
        }

        public async Task<IEnumerable<PlaylistTrack>> GetTracks(ulong id, string playlistName)
        {
            var playlist = await GetPlaylist(id, playlistName);
            if (playlist == null)
                return null;
     
            return await GetTracks(playlist);
        }

        public Task<IEnumerable<PlaylistTrack>> GetTracks(Playlist playlist) => Task.FromResult(context.ServerPlaylistTracks.AsEnumerable().Where(x => x.PlaylistId == playlist.Id));

        public async Task<bool> AddTrack(ulong id, string name, ulong authorId, string track)
        {
            var playlist = await GetPlaylist(id, name);
            if (playlist == null)
                return false;

            return await AddTrack(playlist, authorId, track);
        }

        public async Task<bool> AddTrack(Playlist playlist, ulong authorId, string track)
        {
            var tracks = context.ServerPlaylistTracks.Where(x => x.PlaylistId == playlist.Id);
            if (tracks.FirstOrDefault(x => x.URI == track) != null)
                return false;

            playlist.Tracks.Add(new PlaylistTrack
            {
                AddedById = (long)authorId,
                URI = track
            });

            return await UpdateEntity(playlist);
        }

        public async Task<bool> RemoveTrack(ulong id, string name, int index)
        {
            var tracks = await GetTracks(id, name);
            if (tracks == null)
                return false;
            
            return await RemoveEntity(tracks.ElementAt(index));
        }

        public async Task<bool> RemoveTrack(Playlist playlist, int index)
        {
            var tracks = context.ServerPlaylistTracks.Where(x => x.PlaylistId == playlist.Id).ToArray();
            if (index >= tracks.Length || index < 0)
                return false;

            return await RemoveEntity(tracks[index]);
        }
    }
}
