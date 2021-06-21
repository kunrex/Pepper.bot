using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;

namespace KunalsDiscordBot.Modules.Games
{
    //A game is complex if
    //the players store any game related data
    //example: if the players have their own boards
    public abstract class ComplexBoardGame<T> : Game where T : DiscordPlayer 
    {
        public List<T> players { get; protected set; }
        protected T currentPlayer { get; set; }

        protected abstract void SetUp();
        protected abstract void PlayGame();
        protected abstract Task PrintBoard();
    }
}
