using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Music
{
    public class PlaylistTrack : Entity<int>
    {
        [ForeignKey("PlaylistId")]
        public int PlaylistId { get; set; }

        public long AddedById { get; set; }
        public string URI { get; set; }
    }
}
