using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;

namespace DiscordBotDataBase.Dal.Models.Servers.Models
{
    public partial class ModerationData : Entity<long>
    {
        [ForeignKey("ServerProfileId")]
        public long ServerProfileId { get; set; }

        public long MutedRoleId { get; set; } = 0;
        public long ModeratorRoleId { get; set; } = 0;

        public List<Rule> Rules { get; set; } = new List<Rule>();

        public List<Infraction> Infractions { get; set; } = new List<Infraction>();
        public List<Endorsement> Endorsements { get; set; } = new List<Endorsement>();
        public List<Mute> Mutes { get; set; } = new List<Mute>();

        public List<Ban> Bans { get; set; } = new List<Ban>();
        public List<Kick> Kicks { get; set; } = new List<Kick>();
    }
}
