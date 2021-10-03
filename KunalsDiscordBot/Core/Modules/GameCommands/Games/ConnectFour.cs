using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public sealed class ConnectFour : DiscordGame<Connect4Player, Connect4Communicator>
    {
        public static readonly float inputTime = 1;

        private int[,] board { get; set; }
        private int numberOfCells { get; set; }

        private const string RED = ":red_circle:";
        private const string YELLOW = ":yellow_circle:";
        private const string BLACK = ":black_circle:";
        private const string SPACE = "   ";

        private readonly DiscordChannel channel;
        private DiscordMessage GameMessage { get; set; }

        public ConnectFour(DiscordClient client, List<DiscordMember> players, DiscordChannel channel, int _numberOfCells) : base(client, players)
        {
            this.channel = channel;

            Players = players.Select(x => new Connect4Player(x)).ToList();
            CurrentPlayer = Players[0];

            numberOfCells = Math.Clamp(_numberOfCells, 5, 8);
            board = new int[numberOfCells, numberOfCells];

            SetUp();
        }

        protected async override void SetUp()
        {
            var embed = await GetBoard();
            GameMessage = await channel.SendMessageAsync(embed).ConfigureAwait(false);
            await Task.WhenAll(Players.Select(x => Task.Run(() => x.Ready(channel))));

            PlayGame();
        }

        protected async override Task PrintBoard()
        {
            var embed = await GetBoard();

            await SendMessage($"{CurrentPlayer.member.Mention} its your turn, which column would you like to place a coin in? Select `end` to end the game", embed)
                .ConfigureAwait(false);
        }

        private Task<DiscordEmbedBuilder> GetBoard()
        {
            string description = string.Empty;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    int state = board[i, k];
                    description += Evaluate(state) + SPACE;
                }

                description += "\n";
            }

            var embed = new DiscordEmbedBuilder()
            {
                Title = $"{Players[0].member.Username} vs {Players[1].member.Username}",
                Color = DiscordColor.Blurple,
                Description = description
            }.AddField("Turn :", CurrentPlayer.member.Username, true)
             .AddField($"Player 1: {Players[0].member.Username}", RED, true)
             .AddField($"Player 2: {Players[1].member.Username}", YELLOW, true);

            return Task.FromResult(embed);
        }

        private string Evaluate(int state) => state == 0 ? BLACK : (state == 1 ? RED : YELLOW);

        protected async override void PlayGame()
        {
            try
            {
                while (!GameOver)
                {
                    await PrintBoard();

                    InputResult result = await CurrentPlayer.GetInput(Client, GameMessage, board);

                    if (!result.WasCompleted)
                    {
                        await SendMessage($"{(result.Type == InputResult.ResultType.End ? $"{CurrentPlayer.member.Mention} has ended the game. noob" : $"{CurrentPlayer.member.Mention} has gone AFK")}")
                            .ConfigureAwait(false);

                        GameOver = true;
                        continue;
                    }

                    board[result.Ordinate.y, result.Ordinate.x] = CurrentPlayer == Players[0] ? 1 : 2;
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
                        await SendMessage($"The match between {Players[0].member.Username} and {Players[1].member.Username} ends in a draw").ConfigureAwait(false);

                        GameOver = true;
                        continue;
                    }

                    CurrentPlayer = CurrentPlayer == Players[0] ? Players[1] : Players[0];
                }
            }
            catch (Exception e)
            {
                await CurrentPlayer.SendMessage(GameMessage, $"Unkown error -  {e.Message}  occured").ConfigureAwait(false);
                GameOver = true;
            }
            finally
            {
                await PrintBoard();
                await GameMessage.ClearComponents();

                OnGameOver.Invoke();
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

        private async Task<bool> CheckForWinner()
        {
            for(int i =0;i<board.GetLength(0); i++)
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

        private async Task<bool> Evaluate(Coordinate ordinate, int indent = 1, int direction = 0)
        {
            if (indent == 4)//player won
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

                    Coordinate diagnolDown = ordinate.GetDiagnolDownPosition();

                    if (Coordinate.EvaluatePosition(diagnolDown.x, diagnolDown.y, board.GetLength(0), board.GetLength(1)))
                    {
                        diagnolDown.value = board[diagnolDown.y, diagnolDown.x];
                        if (diagnolDown.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(diagnolDown, indent + 1, 3);
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
                    Coordinate _diagnolDown = ordinate.GetDiagnolDownPosition();

                    if (Coordinate.EvaluatePosition(_diagnolDown.x, _diagnolDown.y, board.GetLength(0), board.GetLength(1)))
                    {
                        _diagnolDown.value = board[_diagnolDown.y, _diagnolDown.x];
                        if (_diagnolDown.value == ordinate.value)
                        {
                            bool isSame = await Evaluate(_diagnolDown, indent + 1, 3);
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

        private async Task SendMessage(string content = null, DiscordEmbedBuilder embed = null)
        {
            if (content != null && embed != null)
                GameMessage = await CurrentPlayer.SendMessage(GameMessage, content, embed);
            else if(content == null)
                GameMessage = await CurrentPlayer.SendMessage(GameMessage, embed);
            else 
                GameMessage = await CurrentPlayer.SendMessage(GameMessage, content);
        }
    }
}
