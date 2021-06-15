using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDatabase
{
    public abstract class Entity
    {
        [Key]
        public int id { get; set; }
    }
}
