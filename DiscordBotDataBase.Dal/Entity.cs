using System;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal
{
    public abstract class Entity<T>  
    {
        public Entity()
        {
            if (!typeof(T).IsNumeric())
                throw new Exception("Invalid Generic for Entity");
        }

        [Key]
        public T Id { get; set; } 
    }
}
