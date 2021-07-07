using System;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

namespace DiscordBotDataBase.Dal.Models.Moderation
{
    public class ModerationProfile : Entity
    {
        public long DiscordId { get; set; }
        public long GuildId { get; set; }

        public List<Infraction> Infractions { get; set; } = new List<Infraction>();
        public List<Endorsement> Endorsements { get; set; } = new List<Endorsement>();

        public List<Ban> Bans { get; set; } = new List<Ban>();
        public List<Kick> Kicks { get; set; } = new List<Kick>();
        public List<Mute> Mutes { get; set; } = new List<Mute>();
    }
}
