using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public sealed class TicTacToe : DiscordGame<TicTacToePlayer, TicTacToeCommunicator>
    {
        public static readonly float inputTime = 1;

        public static readonly string x = ":x:";
        public static readonly string o = ":o:";
        public static readonly string blank = "";

        public const int numberOfCells = 3;
        private readonly DiscordChannel channel;

        private int[,] Board { get; set; }
        private DiscordMessage GameMessage { get; set; }

        public TicTacToe(DiscordClient _client, List<DiscordMember> _players, DiscordChannel _channel) : base(_client, _players)
        {
            channel = _channel;

            Players = _players.Select(x => new TicTacToePlayer(x)).ToList();
            CurrentPlayer = Players[0];
            
            Board = new int[numberOfCells, numberOfCells];

            OnGameOver = new SimpleBotEvent();
            SetUp();
        }

        protected async override void SetUp()
        {
            GameMessage = await channel.SendMessageAsync("Starting Game").ConfigureAwait(false);
            await Task.WhenAll(Players.Select(x => Task.Run(() => x.Ready(channel))));

            PlayGame();
        }

        protected async override Task PrintBoard() => GameMessage = await CurrentPlayer.PrintCompleteBoard(Client, GameMessage, $"{CurrentPlayer.member.Mention} its you're turn, where would you like to play?", Board);

        private async Task<bool> CheckForWinner()
        {
            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int k = 0; k < Board.GetLength(1); k++)
                {
                    if (Board[i, k] == 0)
                        continue;

                    Coordinate ordinate = new Coordinate
                    {
                        x = k,
                        y = i,
                        value = Board[i, k]
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
                while (!GameOver)
                {
                    await PrintBoard();

                    InputResult result = await CurrentPlayer.GetInput(Client, GameMessage);

                    if (!result.WasCompleted)
                    {
                        await SendMessage(result.Type == InputResult.ResultType.Afk ? $"{CurrentPlayer.member.Mention} has gone AFK" : $"{CurrentPlayer.member.Mention} has ended the game. noob").ConfigureAwait(false);

                        GameOver = true;
                        continue; 
                    }

                    Board[result.Ordinate.y, result.Ordinate.x] = CurrentPlayer == Players[0] ? 1 : 2;
                   
                    await CheckForWinner();

                    if (GameOver)
                    {
                        await SendMessage($"{CurrentPlayer.member.Mention} Wins!").ConfigureAwait(false);

                        GameOver = true;
                        continue;
                    }

                    bool draw = await CheckDraw();

                    if (draw)
                    {
                        await SendMessage($"The match between {Players[0].member.Mention} and {Players[1].member.Mention} ends in a draw").ConfigureAwait(false);

                        GameOver = true;
                        continue; 
                    }

                    CurrentPlayer = CurrentPlayer == Players[0] ? Players[1] : Players[0];
                }
            }
            catch (Exception e)
            {
                await SendMessage($"Unkown error -  {e.Message}  occured").ConfigureAwait(false);
                GameOver = true;
            }
            finally
            {
                await CurrentPlayer.PrintCompleteBoard(Client, GameMessage, GameMessage.Content, Board, true);

                OnGameOver.Invoke();
            }
        }

        private Task<bool> CheckDraw()
        {
            for (int i = 0; i < Board.GetLength(0); i++)
            {
                for (int k = 0; k < Board.GetLength(1); k++)
                {
                    if (Board[i, k] == 0)
                        return Task.FromResult(false);
                }
            }

            return Task.FromResult(true);
        }

        private async Task<bool> Evaluate(Coordinate ordinate, int indent = 1, int direction = 0)
        {
            if (indent == 3)//player one
            {
                GameOver = true;
                return true;
            }

            if (ordinate.value == 0)//ignore
                return false;

            switch (direction)
            {
                case 0:
                    Coordinate down = ordinate.GetDownPosition();

                    if (Coordinate.EvaluatePosition(down.x, down.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        down.value = Board[down.y, down.x];
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

                    if (Coordinate.EvaluatePosition(right.x, right.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        right.value = Board[right.y, right.x];
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

                    if (Coordinate.EvaluatePosition(diagnol.x, diagnol.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        diagnol.value = Board[diagnol.y, diagnol.x];
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

                    if (Coordinate.EvaluatePosition(diagnolUp.x, diagnolUp.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        diagnolUp.value = Board[diagnolUp.y, diagnolUp.x];
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

                    if (Coordinate.EvaluatePosition(_down.x, _down.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        _down.value = Board[_down.y, _down.x];
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

                    if (Coordinate.EvaluatePosition(_right.x, _right.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        _right.value = Board[_right.y, _right.x];
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

                    if (Coordinate.EvaluatePosition(_diagnol.x, _diagnol.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        _diagnol.value = Board[_diagnol.y, _diagnol.x];
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

                    if (Coordinate.EvaluatePosition(_diagnolUp.x, _diagnolUp.y, Board.GetLength(0), Board.GetLength(1)))
                    {
                        _diagnolUp.value = Board[_diagnolUp.y, _diagnolUp.x];
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

        private async Task SendMessage(string content = null)
        {
            if (content != null)
                GameMessage = await CurrentPlayer.SendMessage(GameMessage, content);
        }
    }
}
