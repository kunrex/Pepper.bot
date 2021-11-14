using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Music
{
    public class Playlist : Entity<int>
    {
        [ForeignKey("MusicDataId")]
        public long MusicDataId { get; set; }

        public long AuthorId { get; set; }
        public string PlaylistName { get; set; }
        public List<PlaylistTrack> Tracks { get; set; } = new List<PlaylistTrack>();
    }
}
