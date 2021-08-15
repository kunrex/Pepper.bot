using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace KunalsDiscordBot.Modules.Games.Communicators
{
    public class Connect4Communicator : DiscordCommunicator
    {
        public DiscordChannel channel { get; private set; }

        public Connect4Communicator(Regex expression, TimeSpan span, DiscordChannel _channel) : base(expression, span)
        {
            channel = _channel;
        }

        public async override Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data)
        {
            await SendMessage(inputMessage).ConfigureAwait(false);
            var message = await WaitForMessage(interactivity, data.conditions, data.span);

            if (message.TimedOut)
                return afkInputvalue;
            else if (message.Result.Content.ToLower().Equals(data.leaveMessage))
                return quitInputvalue;
            else if (!inputExpression.IsMatch(message.Result.Content))
            {
                await SendMessage(data.regexMatchFailExpression);

                return inputFormatNotFollow;
            }
            else
                return message.Result.Content;
        }

        public async Task<DiscordMessage> SendEmbedToPlayer(DiscordEmbed embed) => await SendEmbedToPlayer(channel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(channel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(channel, message, embed);

    }
}
