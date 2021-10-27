using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models
{
    public partial class FunData : Entity<long>
    {
        [ForeignKey("ServerProfileId")]
        public long ServerProfileId { get; set; }

        public int AllowNSFW { get; set; } = 0;

        public int AllowSpamCommand { get; set; } = 0;
        public int AllowGhostCommand { get; set; } = 0;
        public int AllowActCommand { get; set; } = 0;
    }
}
