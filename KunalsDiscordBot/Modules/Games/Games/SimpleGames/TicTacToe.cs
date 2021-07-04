//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Modules.Games.Simple
{
    public sealed class TicTacToe : SimpleBoardGame
    {
        private const float time = 60;

        public int numberOfCells { get; private set; }

        private const string X = ":x:";
        private const string O = ":o:";
        private const string BLACK = ":black_large_square:";

        public TicTacToe(CommandContext _ctx, DiscordUser user1, DiscordUser user2, int _numberOfCells)
        {
            ctx = _ctx;
            player1 = user1;
            player2 = user2;

            numberOfCells = _numberOfCells;
            numberOfCells = System.Math.Clamp(numberOfCells, 3, 5);

            board = new int[numberOfCells, numberOfCells];

            currentUser = player1;
            PlayGame();
        }
        protected async override Task<bool> PrintBoard()
        {
            string description = string.Empty;
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    int state = board[i, k];
                    description += (Evaluate(state));
                }

                description += "\n";
            }

            var Embed = new DiscordEmbedBuilder()
            {
                Title = $"{player1.Username} vs {player2.Username}",
                Color = DiscordColor.Blurple,
                Description = description
            };
            Embed.AddField("Turn :", currentUser.Username, true);

            Embed.AddField("Player 1 :", X, true);
            Embed.AddField("Player 2 :", O, true);


            await ctx.Channel.SendMessageAsync(embed: Embed).ConfigureAwait(false);

            return true;

            string Evaluate(int state) => state == 0 ? BLACK : (state == 1 ? X : O);
        }


        protected async override Task<bool> CheckForWinner()
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

        protected async override Task<InputResult> GetInput()
        {
            await ctx.Channel.SendMessageAsync($"{currentUser.Mention}, its your turn, type the slot in which you want to place a marker.\n Ex: a1. Type \"end\" to end the game ").ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == currentUser, TimeSpan.FromSeconds(time)).ConfigureAwait(false);

            if (message.TimedOut)
                return new InputResult { wasCompleted = false, type = InputResult.Type.afk };
            else if (message.Result.Content.Equals("end"))
                return new InputResult { wasCompleted = false, type = InputResult.Type.end };
            else if (CanBeParsed(message.Result.Content, out int x, out int y))
            {
                board[x, y] = currentUser == player1 ? 1 : 2;
                return new InputResult { wasCompleted = true, type = InputResult.Type.valid };
            }
            else
                return new InputResult { wasCompleted = true, type = InputResult.Type.inValid };
        }

        protected async override void PlayGame()
        {
            try
            {
                while (!gameOver)
                {
                    await PrintBoard();

                    InputResult completed = await GetInput();

                    if (!completed.wasCompleted)
                    {
                        await ctx.Channel.SendMessageAsync($"{(completed.type == InputResult.Type.end ? $"{currentUser.Mention} has ended the game. noob" : $"{currentUser.Mention} has gone AFK")}").ConfigureAwait(false);
                        break;
                    }
                    else if(completed.type == InputResult.Type.inValid)
                        await ctx.Channel.SendMessageAsync($"{currentUser.Mention} thats an invalid answer dum dum, now you lose a turn").ConfigureAwait(false);

                    await CheckForWinner();

                    if (gameOver)
                    {
                        await ctx.Channel.SendMessageAsync($"{currentUser.Mention} Wins!").ConfigureAwait(false);
                        await PrintBoard();
                        return;
                    }

                    bool draw = await CheckDraw();

                    if (draw)
                    {
                        await ctx.Channel.SendMessageAsync($"{player1.Username} and {player2.Username} ends in a draw").ConfigureAwait(false);
                        await PrintBoard();
                        return;
                    }

                    currentUser = currentUser == player1 ? player2 : player1;
                }
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync($"Unkown error -  {e.Message}  occured, <@{Bot.KunalsID}>").ConfigureAwait(false);
                gameOver = true;
                return;
            }
        }

        protected async override Task<bool> CheckDraw()
        {
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    if (board[i, k] == 0)
                        return false;
                }
            }

            await Task.CompletedTask;
            return true;
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


        private bool CanBeParsed(string value, out int x, out int y)
        {
            if(value.Length != 2)
            {
                x = 0;
                y = 0;
                return false;
            }

            int c1 = value[0] - 97;
            int c2;

            if (!int.TryParse(value[1].ToString(), out int val))
            {
                x = 0;
                y = 0;
                return false;
            }

            c2 = int.Parse(value[1].ToString()) - 1;

            if(c1 >= 0 && c1 < board.GetLength(0) && c2 >= 0 && c2 < board.GetLength(1))
            {
                x = c1;
                y = c2;

                return board[x, y] == 0;
            }
            else
            {
                x = 0;
                y = 0;
                return false;
            }
        }
    }
}
