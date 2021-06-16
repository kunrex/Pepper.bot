using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; } 
    }
}
