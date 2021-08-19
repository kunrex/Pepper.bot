using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class TicTacToePlayer : DiscordPlayer<TicTacToeCommunicator>
    {
        public TicTacToePlayer(DiscordMember _member) : base(_member)
        {

        }

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new TicTacToeCommunicator(new Regex("([a-e] [1-5])"), TimeSpan.FromMinutes(TicTacToe.inputTime), channel);

            return Task.FromResult(true);
        }

        public async Task<InputResult> GetInput(DiscordClient client, int[,] board)
        {
            while (true)
            {
                var result = await communicator.Input(client.GetInteractivity(), $"{member.Mention}, its you're turn. Where would you like to play? \nUse the format <row> <column>. " +
                        $"Ex: a 1 (the top left square). Type `end` to end the game.", new InputData
                {
                    conditions = x => x.Channel == communicator.channel && x.Author.Id == member.Id,
                    span = TimeSpan.FromMinutes(TicTacToe.inputTime),
                    leaveMessage = "end",
                    regexMatchFailExpression = $"{member.Username}, do you mind using the input format?"
                });

                if (result.Equals(DiscordCommunicator.afkInputvalue))
                    return new InputResult { wasCompleted = false, type = InputResult.Type.afk };
                else if (result.Equals(DiscordCommunicator.quitInputvalue))
                    return new InputResult { wasCompleted = false, type = InputResult.Type.end };
                else if (result.Equals(DiscordCommunicator.inputFormatNotFollow))
                    continue;
                else if (TryExtract(result, board, out Coordinate ordinate))
                    return new InputResult { wasCompleted = true, type = InputResult.Type.valid, ordinate = ordinate };
                else
                    await communicator.SendMessage($"{member.Mention}, thats not valid input");
            }
        }

        private bool TryExtract(string value, int[,] board, out Coordinate ordinate)
        {
            ordinate = new Coordinate
            {
                y = value[0] - 97,
                x = int.Parse(value[2].ToString()) - 1
            };

            if (ordinate.x < 0 || ordinate.x >= board.GetLength(0) || ordinate.y < 0 || ordinate.y >= board.GetLength(1))
                return false;
            if (board[ordinate.y, ordinate.x] != 0)
                return false;

            return true;
        }

        public async Task SendMessage(string message) => await communicator.SendMessage(message);
        public async Task SendMessage(DiscordEmbedBuilder embed) => await communicator.SendEmbedToPlayer(embed);
        public async Task SendMessage(string message, DiscordEmbedBuilder embed) => await communicator.SendMessage(message, embed);
    }
}
