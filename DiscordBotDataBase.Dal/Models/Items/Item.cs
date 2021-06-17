using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal.Models.Items
{
    public class Item : Entity
    {
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
