using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Profile.Boosts
{
    public class BoostData : Entity
    {
        public string BoosteName { get; set; }
        public int BoostValue { get; set; }
        public int BoostTime { get; set; }

        public string BoostStartTime { get; set; }

        [ForeignKey("ProfileId")]
        public int ProfileId { get; set; }
    }
}
