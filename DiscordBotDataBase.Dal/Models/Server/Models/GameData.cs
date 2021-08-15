using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Servers.Models
{
    public partial class GameData : Entity<long>
    {
        [ForeignKey("ServerProfileId")]
        public long ServerProfileId { get; set; }

        public long Connect4Channel { get; set; } = 0;
        public long TicTacToeChannel { get; set; } = 0;
    }
}
