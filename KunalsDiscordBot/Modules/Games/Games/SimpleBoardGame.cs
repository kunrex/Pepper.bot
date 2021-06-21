//System name spaces
using System.Threading.Tasks;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

using KunalsDiscordBot.Modules.Games.Players;

namespace KunalsDiscordBot.Modules.Games
{

    //A game is as a simple board game
    //if the players don't themselves store any data
    //and can be represented just by the DiscordUser type
    //for example: the players don't have their own boards
    public abstract class SimpleBoardGame : Game
    {
        public CommandContext ctx { get; protected set; }
        public DiscordUser player1 { get; protected set; }
        public DiscordUser player2 { get; protected set; }

        protected DiscordUser currentUser { get; set; }

        protected bool gameOver { get; set; }

        public int[,] board { get; protected set; }

        protected abstract Task<bool> PrintBoard();

        protected abstract Task<InputResult> GetInput();

        protected abstract Task<bool> CheckDraw();

        protected abstract Task<bool> CheckForWinner();

        protected abstract void PlayGame();
    }
}
