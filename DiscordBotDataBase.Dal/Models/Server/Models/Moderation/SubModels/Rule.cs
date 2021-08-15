using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class Rule : Entity<int>
    {
        [ForeignKey("ModerationDataId")]
        public int ModerationDataId { get; set; }

        public string RuleContent { get; set; }
    }
}
