using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class Ban : Entity<int>
    {
        [ForeignKey("ModerationDataId")]
        public int ModerationDataId { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;

        public long ModeratorID { get; set; }
        public long GuildID { get; set; }
        public long UserId { get; set; }
    }
}
