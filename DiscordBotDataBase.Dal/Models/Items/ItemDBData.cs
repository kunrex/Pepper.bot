using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal.Models.Items
{
    public class ItemDBData : Entity
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
