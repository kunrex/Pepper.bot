using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class PenaltyCommunicator : DiscordCommunicator, ITextInputCommunicator
    {
        private readonly DiscordChannel channel;
        public DiscordChannel Channel { get => channel; }

        public PenaltyCommunicator(DiscordChannel _channel)
        {
            inputExpression = new Regex("([1-3])");
            channel = _channel;
        }

        private readonly Regex inputExpression;
        public Regex InputExpression { get => inputExpression; }

        public async Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData inputData)
        {
            if (inputData.InputType != InputType.Message)
                throw new InvalidOperationException();

            var result = await WaitForMessage(interactivity, inputData.Conditions, inputData.Span);
            if (result.TimedOut)
                return afkInputvalue;
            else if (!inputExpression.IsMatch(result.Result.Content))
                return inputFormatNotFollow;

            return result.Result.Content;
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessageBuilder message) => await message.SendAsync(channel);
        public async Task<DiscordMessage> EditMessage(DiscordMessage message, DiscordMessageBuilder newMessage) => await message.ModifyAsync(newMessage);
    }
}
