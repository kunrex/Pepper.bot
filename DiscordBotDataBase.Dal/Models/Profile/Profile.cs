using System;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Items;

namespace DiscordBotDataBase.Dal.Models.Profile
{
    public class Profile : Entity
    {
        public long DiscordUserID { get; set; }
        public int XP { get; set; }
        public string Name { get; set; }

        public int Coins { get; set; }
        public int CoinsBank { get; set; }
        public int CoinsBankMax { get; set; }

        public string Job { get; set; }

        public List<ItemDBData> Items { get; set; } = new List<ItemDBData>();
    }
}
