using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class Rule : Entity<int>
    {
        public string RuleContent { get; set; }

        [ForeignKey("ModerationDataId")]
        public long ModerationDataId { get; set; }
    }
}
