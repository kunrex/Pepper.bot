using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDB.Dal
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
    }
}
