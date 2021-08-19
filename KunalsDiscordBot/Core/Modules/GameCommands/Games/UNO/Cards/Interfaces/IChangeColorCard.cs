using System;
namespace KunalsDiscordBot.Core.Modules.GameCommands.UNO.Cards
{
    public interface IChangeColorCard
    {
        public abstract CardColor colorToChange { get; set; }
    }
}
