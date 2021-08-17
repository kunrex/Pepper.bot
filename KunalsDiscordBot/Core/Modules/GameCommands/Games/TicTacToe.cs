//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Players;
using KunalsDiscordBot.Modules.Games.Communicators;
using DSharpPlus;
using System.Linq;

namespace KunalsDiscordBot.Modules.Games
{
    public sealed class TicTacToe : DiscordGame<TicTacToePlayer, TicTacToeCommunicator>
    {
        public static readonly float inputTime = 1;

        public int numberOfCells { get; private set; }

        private const string X = ":x:";
        private const string O = ":o:";
        private const string BLACK = ":black_large_square:";

        private int[,] board { get; set; }
        private readonly DiscordChannel channel;
        private bool gameOver { get; set; }

        public TicTacToe(DiscordClient _client, List<DiscordMember> _players, DiscordChannel _channel, int _numberOfCells) : base(_client, _players)
        {
            channel = _channel;
            client = _client;

            players = _players.Select(x => new TicTacToePlayer(x)).ToList();
            currentPlayer = players[0];
            
            numberOfCells = System.Math.Clamp(_numberOfCells, 3, 5);
            board = new int[numberOfCells, numberOfCells];

            SetUp();
        }

        protected async override void SetUp()
        {
            await Task.WhenAll(players.Select(x => Task.Run(() => x.Ready(channel))));
            await currentPlayer.SendMessage("Starting Game");

            PlayGame();
        }

        protected async override Task PrintBoard()
        {
            string description = string.Empty;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    int state = board[i, k];
                    description += Evaluate(state);
                }

                description += "\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"{players[0].member.Username} vs {players[1].member.Username}",
                Color = DiscordColor.Blurple,
                Description = description
            };
            embed.AddField("Turn :", currentPlayer.member.Username, true);

            embed.AddField("Player 1 :", X, true);
            embed.AddField("Player 2 :", O, true);


            await currentPlayer.SendMessage(embed).ConfigureAwait(false);
        }

        private string Evaluate(int state) => state == 0 ? BLACK : (state == 1 ? X : O);

        private async Task<bool> CheckForWinner()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    if (board[i, k] == 0)
                        continue;

                    Coordinate ordinate = new Coordinate
                    {
                        x = k,
                        y = i,
                        value = board[i, k]
                    };

                    bool isIn4 = await Evaluate(ordinate);

                    if (isIn4)
                        return true;
                }
            }

            return false;
        }

        protected async override void PlayGame()
        {
            try
            {
                while (!gameOver)
                {
                    await PrintBoard();

                    InputResult completed = await currentPlayer.GetInput(client, board);

                    if (!completed.wasCompleted)
                    {
                        await currentPlayer.SendMessage($"{(completed.type == InputResult.Type.end ? $"{currentPlayer.member.Mention} has ended the game. noob" : $"{currentPlayer.member.Mention} has gone AFK")}").ConfigureAwait(false);
                        break;
                    }

                    board[completed.ordinate.y, completed.ordinate.x] = currentPlayer == players[0] ? 1 : 2;
                   
                    await CheckForWinner();

                    if (gameOver)
                    {
                        await currentPlayer.SendMessage($"{currentPlayer.member.Mention} Wins!").ConfigureAwait(false);
                        await PrintBoard();
                        return;
                    }

                    bool draw = await CheckDraw();

                    if (draw)
                    {
                        await currentPlayer.SendMessage($"{players[0].member.Username} and {players[1].member.Username} ends in a draw").ConfigureAwait(false);
                        await PrintBoard();
                        return;
                    }

                    currentPlayer = currentPlayer == players[0] ? players[1] : players[0];
                }
            }
            catch (Exception e)
            {
                await currentPlayer.SendMessage($"Unkown error -  {e.Message}  occured").ConfigureAwait(false);
                gameOver = true;
                return;
            }
        }

        private Task<bool> CheckDraw()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    if (board[i, k] == 0)
                        return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        private async Task<bool> Evaluate(Coordinate ordinate, int indent = 1, int direction = 0)
        {
            if (indent == 3)//player one
            {
                gameOver = true;
                return true;
            }

            if (ordinate.value == 0)//ignore
                return false;

            switch (direction)
            {
                case 0:
                    Coordinate down = ordinate.GetDownPosition();

                    if (Coordinate.EvaluatePosition(down.x, down.y, board.GetLength(0), board.GetLength(1)))
                    {
                        down.value = board[down.y, down.x];
                        if (down.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(down, indent + 1, 1);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    Coordinate right = ordinate.GetRightPosition();

                    if (Coordinate.EvaluatePosition(right.x, right.y, board.GetLength(0), board.GetLength(1)))
                    {
                        right.value = board[right.y, right.x];
                        if (right.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(right, indent + 1, 2);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    Coordinate diagnol = ordinate.GetDiagnolDownPosition();

                    if (Coordinate.EvaluatePosition(diagnol.x, diagnol.y, board.GetLength(0), board.GetLength(1)))
                    {
                        diagnol.value = board[diagnol.y, diagnol.x];
                        if (diagnol.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(diagnol, indent + 1, 3);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    Coordinate diagnolUp = ordinate.GetDiagnolUpPosition();

                    if (Coordinate.EvaluatePosition(diagnolUp.x, diagnolUp.y, board.GetLength(0), board.GetLength(1)))
                    {
                        diagnolUp.value = board[diagnolUp.y, diagnolUp.x];
                        if (diagnolUp.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(diagnolUp, indent + 1, 4);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                case 1:
                    Coordinate _down = ordinate.GetDownPosition();

                    if (Coordinate.EvaluatePosition(_down.x, _down.y, board.GetLength(0), board.GetLength(1)))
                    {
                        _down.value = board[_down.y, _down.x];
                        if (_down.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(_down, indent + 1, 1);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                case 2:
                    Coordinate _right = ordinate.GetRightPosition();

                    if (Coordinate.EvaluatePosition(_right.x, _right.y, board.GetLength(0), board.GetLength(1)))
                    {
                        _right.value = board[_right.y, _right.x];
                        if (_right.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(_right, indent + 1, 2);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                case 3:
                    Coordinate _diagnol = ordinate.GetDiagnolDownPosition();

                    if (Coordinate.EvaluatePosition(_diagnol.x, _diagnol.y, board.GetLength(0), board.GetLength(1)))
                    {
                        _diagnol.value = board[_diagnol.y, _diagnol.x];
                        if (_diagnol.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(_diagnol, indent + 1, 3);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    return false;

                case 4:
                    Coordinate _diagnolUp = ordinate.GetDiagnolUpPosition();

                    if (Coordinate.EvaluatePosition(_diagnolUp.x, _diagnolUp.y, board.GetLength(0), board.GetLength(1)))
                    {
                        _diagnolUp.value = board[_diagnolUp.y, _diagnolUp.x];
                        if (_diagnolUp.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(_diagnolUp, indent + 1, 4);
                            if (isSame)
                            {
                                return true;
                            }
                        }
                    }

                    return false;
            }

            return false;
        }
    }
}
