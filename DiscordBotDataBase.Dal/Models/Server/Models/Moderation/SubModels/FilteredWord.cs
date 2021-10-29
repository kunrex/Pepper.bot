using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models.Moderation
{
    public class FilteredWord : Entity<int>
    {
        public bool AddInfraction { get; set; }
        public string Word { get; set; }

        [ForeignKey("ModerationDataId")]
        public long ModerationDataId { get; set; }
    }
}
