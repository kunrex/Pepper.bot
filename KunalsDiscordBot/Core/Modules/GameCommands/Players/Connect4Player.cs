using System;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            communicator = new Connect4Communicator(new Regex("([1-8])"), TimeSpan.FromMinutes(ConnectFour.inputTime));

            return Task.FromResult(true);
        }

        public async Task<InputResult> GetInput(DiscordClient client, DiscordMessage message, int[,] board)
        {
            var inputValues = new Dictionary<string, (string, string)>();
            foreach (var column in GetValidColumns(board))
            {
                var toString = column.ToString();
                inputValues.Add(toString, (toString, toString));
            }

            inputValues.Add("end", ("end", DiscordCommunicator.quitInputvalue));

            var result = await communicator.Input(client.GetInteractivity(), message, member, new InputData
            {
                Span = TimeSpan.FromMinutes(ConnectFour.inputTime),
                InputType = InputType.Dropdown,
                ExtraInputValues = inputValues
            });

            if (result.Equals(DiscordCommunicator.afkInputvalue))
                return new InputResult { WasCompleted = false, Type = InputResult.ResultType.Afk };
            else if (result.Equals(DiscordCommunicator.quitInputvalue))
                return new InputResult { WasCompleted = false, Type = InputResult.ResultType.End };
            else
                return new InputResult { WasCompleted = true, Type = InputResult.ResultType.Valid, Ordinate = Extract(result, board)};
        }

        private List<int> GetValidColumns(int[,] board)
        {
            List<int> validColumns = new List<int>();

            for (int i = 0; i < board.GetLength(1); i++)
                if (board[0, i] == 0)
                    validColumns.Add(i + 1);

            return validColumns;
        }

        private Coordinate Extract(string value, int[,] board)
        {
            var ordinate = new Coordinate
            {
                x = int.Parse(value[0].ToString()) - 1,
            };

            int nearestIndex = -1; bool found = false;

            for (int i = 0; i < board.GetLength(1); i++)
            {
                if (board[i, ordinate.x] == 0 && !found)
                    nearestIndex++;
                else if (board[i, ordinate.x] != 0)
                    found = true;
            }

            ordinate.y = nearestIndex;

            return ordinate;
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content) => await communicator.SendMessage(message, content);
        public async Task<DiscordMessage> SendMessage(DiscordMessage message, DiscordEmbedBuilder embed) => await communicator.SendMessage(message, embed);
        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content, DiscordEmbedBuilder embed) => await communicator.SendMessage(message, content, embed);
    }
}
