using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Profile.Boosts
{
    public class BoostData : Entity<int>
    {
        public string BoosteName { get; set; }

        public int BoostValue { get; set; }

        public string BoostTime { get; set; }
        public string BoostStartTime { get; set; }

        [ForeignKey("ProfileId")]
        public long ProfileId { get; set; }
    }
}
