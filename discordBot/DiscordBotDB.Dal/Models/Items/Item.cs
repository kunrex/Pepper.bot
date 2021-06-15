using System;

namespace DiscordBotDB.Dal.Models.Items
{
    public class Item : Entity
    {
        public string name { get; set; }

        public string description { get; set; }
    }
}
