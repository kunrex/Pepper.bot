using System;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal
{
    public abstract class Entity<T>  : IEntity
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
