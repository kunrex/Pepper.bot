using System;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;

namespace DiscordBotDataBase.Dal.Models.Profile
{
    public class Profile : Entity<long>
    {
        public int Level { get; set; }
        public int XP { get; set; }
        public string Name { get; set; }

        public int SafeMode { get; set; }

        public int Coins { get; set; }
        public int CoinsBank { get; set; }
        public int CoinsBankMax { get; set; }

        public string Job { get; set; }
        public string PrevWorkDate { get; set; }

        public List<ItemDBData> Items { get; set; } = new List<ItemDBData>();
        public List<BoostData> Boosts { get; set; } = new List<BoostData>();
    }
}
