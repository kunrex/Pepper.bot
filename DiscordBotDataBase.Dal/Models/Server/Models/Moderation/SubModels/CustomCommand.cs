using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class CustomCommand : Entity<int>
    {
        public string CommandName { get; set; }
        public string CommandContent { get; set; }

        [ForeignKey("ModerationDataId")]
        public long ModerationDataId { get; set; }
    }
}
