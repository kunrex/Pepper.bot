using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Communicators;
using DSharpPlus;

namespace KunalsDiscordBot.Modules.Games
{
    public abstract class DiscordGame<Player, Communicator> : Game where Communicator : DiscordCommunicator
        where Player : DiscordPlayer<Communicator> 
    {
        public static readonly int maxSpectators = 10;

        public DiscordGame(DiscordClient _client, List<DiscordMember> _players)
        {
            client = _client;
        }

        public delegate void Event();
        public Event OnGameOver { get; set; }

        public DiscordClient client { get; protected set; }

        public List<Player> players { get; protected set; }
        protected Player currentPlayer { get; set; }
        public bool GameOver { get; protected set; } = false;

        protected abstract void SetUp();
        protected abstract void PlayGame();
        protected abstract Task PrintBoard();
    }
}
