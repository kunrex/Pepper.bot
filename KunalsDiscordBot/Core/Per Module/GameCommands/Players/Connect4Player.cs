using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public class Connect4Player : DiscordPlayer<Connect4Communicator>
    {
        public Connect4Player(DiscordMember _member) : base(_member)
        {

        }

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new Connect4Communicator(new Regex("([1-8])"), TimeSpan.FromMinutes(ConnectFour.inputTime), channel);

            return Task.FromResult(true);
        }

        public async Task<InputResult> GetInput(DiscordClient client, int[,] board)
        {
            while (true)
            {
                var result = await communicator.Input(client.GetInteractivity(), $"{member.Mention}, its you're turn. Where would you like to play? \nJust type in the column number. " +
                        $"The columns increase from left to right starting from 1. Type `end` to end the game.", new InputData
                        {
                            conditions = x => x.Channel == communicator.channel && x.Author.Id == member.Id,
                            span = TimeSpan.FromMinutes(ConnectFour.inputTime),
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
                x = int.Parse(value[0].ToString()) - 1,
            };

            if (ordinate.x < 0 || ordinate.x >= board.GetLength(0))
                return false;

            int nearestIndex = -1; bool found = false;

            for (int i = 0; i < board.GetLength(1); i++)
            {
                if (board[i, ordinate.x] == 0 && !found)
                    nearestIndex++;
                else if (board[i, ordinate.x] != 0)
                    found = true;
            }

            ordinate.y = nearestIndex;

            return nearestIndex != -1;
        }


        public async Task SendMessage(string message) => await communicator.SendMessage(message);
        public async Task SendMessage(DiscordEmbedBuilder embed) => await communicator.SendEmbedToPlayer(embed);
        public async Task SendMessage(string message, DiscordEmbedBuilder embed) => await communicator.SendMessage(message, embed);
    }
}
