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
    public class TicTacToePlayer : DiscordPlayer<TicTacToeCommunicator>
    {
        public TicTacToePlayer(DiscordMember _member) : base(_member)
        {

        }

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new TicTacToeCommunicator();

            return Task.FromResult(true);
        }

        public async Task<InputResult> GetInput(DiscordClient client, DiscordMessage message)
        {
            var result = await communicator.Input(client.GetInteractivity(), message, member, new InputData
            {
                Span = TimeSpan.FromMinutes(ConnectFour.inputTime),
                InputType = InputType.Button,
            });

            if (result.Equals(DiscordCommunicator.afkInputvalue))
                return new InputResult { WasCompleted = false, Type = InputResult.ResultType.Afk };
            else if(result.Equals(DiscordCommunicator.quitInputvalue))
                return new InputResult { WasCompleted = false, Type = InputResult.ResultType.End };
            else
                return new InputResult { WasCompleted = true, Type = InputResult.ResultType.Valid, Ordinate = Extract(result)};
        }

        private Coordinate Extract(string value) => new Coordinate { x = int.Parse(value[2].ToString()), y = int.Parse(value[0].ToString()) };

        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content) => await communicator.SendMessage(message, content);
        public async Task<DiscordMessage> PrintCompleteBoard(DiscordClient client, DiscordMessage message, string content, int[,] board, bool disableAll = false) => await communicator.PrintCompleteBoard(client, message, content, board, disableAll);
    }
}
