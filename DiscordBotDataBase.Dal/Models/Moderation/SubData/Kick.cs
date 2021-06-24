using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Moderation.SubData
{
    public class Kick : Entity
    {
        public string Reason { get; set; } = string.Empty;

        [ForeignKey("ModerationProfileId")]
        public int ModerationProfileId { get; set; }
    }
}
