using System;
namespace KunalsDiscordBot.Modules.Games.UNO.Cards
{
    public interface IChangeColorCard
    {
        public abstract CardColor colorToChange { get; set; }
    }
}
