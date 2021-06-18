using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscordBotDataBase.Dal.Models.Items
{
    public class ItemDBData : Entity
    {
        public string Name { get; set; }
        public int Count { get; set; }

        [ForeignKey("ProfileId")]
        public int ProfileId { get; set; }
    }
}
