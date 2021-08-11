using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Communicators;

namespace KunalsDiscordBot.Modules.Games
{
    public abstract class ComplexBoardGame<Player, Communicator> : Game where Communicator : DiscordCommunicator
        where Player : DiscordPlayer<Communicator> 
    {
        public List<Player> players { get; protected set; }
        protected Player currentPlayer { get; set; }

        protected abstract void SetUp();
        protected abstract void PlayGame();
        protected abstract Task PrintBoard();
    }
}
