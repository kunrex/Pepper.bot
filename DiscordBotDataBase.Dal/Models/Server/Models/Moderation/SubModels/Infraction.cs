using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class Infraction : Entity<int>
    { 
        public string Reason { get; set; } = string.Empty;

        public long ModeratorID { get; set; }
        public long GuildID { get; set; }
        public long UserId { get; set; }

        [ForeignKey("ModerationDataId")]
        public long ModerationDataId { get; set; }
    }
}
