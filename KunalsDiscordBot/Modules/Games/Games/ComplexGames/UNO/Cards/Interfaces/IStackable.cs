using System;

namespace KunalsDiscordBot.Modules.Games.Complex.UNO.Cards
{
    public interface IStackable
    {
        public CardType stackables { get; }

        public bool Stack(Card card);
    }
}
