using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class BattleShipCommunicator : DiscordCommunicator, ITextInputCommunicator
    {
        public DiscordChannel DMChannel { get; private set; }

        private Regex battleShipInputExpression;
        public Regex BattleShipInputExpression { get => battleShipInputExpression; }

        private Regex inputExpression;
        public Regex InputExpression { get => inputExpression; }

        public BattleShipCommunicator(DiscordChannel _DMChannel, Regex _inputExpression, Regex _battleShipInputExpression)  
        {
            DMChannel = _DMChannel;

            inputExpression = _inputExpression;
            battleShipInputExpression = _battleShipInputExpression;
        }

        public async Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData inputData)
        {
            await SendMessage(inputMessage);

            var message = await WaitForMessage(interactivity, inputData.Conditions, inputData.Span);

            if (message.TimedOut)
                return afkInputvalue;
            else if (message.Result.Content.ToLower().Equals(inputData.LeaveMessage))
                return quitInputvalue;
            else if (!InputExpression.IsMatch(message.Result.Content))
            {
                await SendMessage(inputData.RegexMatchFailExpression);

                return inputFormatNotFollow;
            }

            return message.Result.Content;
        }

        public async Task<string> ShipInput(InteractivityExtension interactivity, string inputMessage, InputData inputData)
        {
            await SendMessage(inputMessage);

            var message = await WaitForMessage(interactivity, inputData.Conditions, inputData.Span);

            if (message.TimedOut)
                return afkInputvalue;
            else if (message.Result.Content.ToLower().Equals(inputData.LeaveMessage))
                return quitInputvalue;
            else if (!BattleShipInputExpression.IsMatch(message.Result.Content))
            {
                await SendMessage(inputData.RegexMatchFailExpression);

                return inputFormatNotFollow;
            }

            return message.Result.Content;
        }

        public async Task EditMessage(DiscordMessage message, string newMessage) => await message.ModifyAsync(newMessage);
        public async Task EditMessage(DiscordMessage message, DiscordEmbed embed) => await message.ModifyAsync(embed);
        public async Task EditMessage(DiscordMessage message, string newMessage, DiscordEmbed embed) => await message.ModifyAsync(newMessage, embed);

        public async Task<DiscordMessage> SendMessage(DiscordEmbed embed) => await SendEmbedToPlayer(DMChannel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(DMChannel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(DMChannel, message, embed);
    }
}
