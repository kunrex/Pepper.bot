using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models
{
    public partial class AIChatData : Entity<long>
    {
        [ForeignKey("ServerProfileId")]
        public long ServerProfileId { get; set; }

        public int Enabled { get; set; } = 0;
        public long AIChatChannelID { get; set; } = 0;
    }
}
