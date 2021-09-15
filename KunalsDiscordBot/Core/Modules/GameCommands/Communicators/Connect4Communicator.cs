using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class Connect4Communicator : DiscordCommunicator, IComponentInputCommunicator
    {
        public Connect4Communicator()
        {

        }

        public async Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData inputData)
        {
            if (inputData.InputType == InputType.Message || inputData.InputType == InputType.Button)
                throw new InvalidOperationException();

            var builder = new DiscordMessageBuilder().WithContent(message.Content).AddEmbeds(message.Embeds);

            builder.AddComponents(new DiscordSelectComponent("Input", "Choose a column", inputData.ExtraInputValues.Select(x => new DiscordSelectComponentOption(x.Key, x.Value.Item1))));
            message = await message.ModifyAsync(builder);

            var result = await WaitForSelection(interactivity, message, user, "Input", inputData.Span);
            if (result.TimedOut)
                return afkInputvalue;
            else
                return inputData.ExtraInputValues.First(x => x.Value.Item1 == result.Result.Values[0]).Value.Item2;
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessage message, DiscordEmbed embed) => await ModifyMessage(message, embed);
        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content) => await ModifyMessage(message, content);
        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content, DiscordEmbed embed) => await ModifyMessage(message, content, embed);
    }
}
