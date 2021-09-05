using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Profile.Boosts
{
    public class BoostData : Entity<int>
    {
        public string Name { get; set; }

        public int PercentageIncrease { get; set; }

        public string TimeSpan { get; set; }
        public string StartTime { get; set; }

        [ForeignKey("ProfileId")]
        public long ProfileId { get; set; }
    }
}
