using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Moderation.SubData
{
    public class Mute : Entity
    {
        public string Reason { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;

        public long ModeratorID { get; set; }

        [ForeignKey("ModerationProfileId")]
        public int ModerationProfileId { get; set; }
    }
}
