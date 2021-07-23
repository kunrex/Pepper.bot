using System;
namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public interface IChangeColorCard
    {
        public abstract CardColor colorToChange { get; set; }
    }
}
