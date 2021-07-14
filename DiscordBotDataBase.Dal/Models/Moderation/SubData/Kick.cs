using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Moderation.SubData
{
    public class Kick : Entity
    {
        public string Reason { get; set; } = string.Empty;

        public long ModeratorID { get; set; }
        public long GuildID { get; set; }

        [ForeignKey("ModerationProfileId")]
        public int ModerationProfileId { get; set; }
    }
}
