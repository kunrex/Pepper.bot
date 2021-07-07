using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Moderation.SubData
{
    public class Rule : Entity
    {
        public string RuleContent { get; set; }

        [ForeignKey("ServerProfileId")]
        public int ServerProfileId { get; set; }
    }
}
