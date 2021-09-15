using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public abstract class DiscordGame<Player, Communicator> : Game where Communicator : DiscordCommunicator
        where Player : DiscordPlayer<Communicator> 
    {
        public static readonly int maxSpectators = 10;

        public DiscordGame(DiscordClient _client, List<DiscordMember> _players)
        {
            Client = _client;
        }

        public delegate void Event();
        public Event OnGameOver { get; set; }

        public DiscordClient Client { get; protected set; }

        public List<Player> Players { get; protected set; }
        protected Player CurrentPlayer { get; set; }
        public bool GameOver { get; protected set; } = false;

        protected abstract void SetUp();
        protected abstract void PlayGame();
        protected abstract Task PrintBoard();
    }
}
