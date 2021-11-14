using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotDataBase.Dal.Models.Servers.Models.Music;

namespace DiscordBotDataBase.Dal.Models.Servers.Models
{
    public partial class MusicData : Entity<long> 
    {
        [ForeignKey("ServerProfileId")]
        public long ServerProfileId { get; set; }

        public int UseDJRoleEnforcement { get; set; } = 0;
        public long DJRoleId { get; set; } = 0;

        public List<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
