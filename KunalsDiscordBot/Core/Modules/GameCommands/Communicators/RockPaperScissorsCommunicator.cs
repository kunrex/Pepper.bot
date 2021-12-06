using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class RockPaperScissorsCommunicator : DiscordCommunicator, IComponentInputCommunicator
    {
        private readonly DiscordChannel channel;
        public DiscordChannel Channel { get => channel; }

        public RockPaperScissorsCommunicator(DiscordChannel _channel)
        {
            channel = _channel;
        }

        public async Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData data)
        {
            if (data.InputType == InputType.Message || data.InputType == InputType.Button)
                throw new InvalidOperationException();

            var result = await WaitForSelection(interactivity, message, user, data.ExtraInputValues["id"].Item1, data.Span);
            return result.TimedOut ? afkInputvalue : result.Result.Values[0];
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessageBuilder message) => await message.SendAsync(channel);
        public async Task<DiscordMessage> EditMessage(DiscordMessage message, DiscordEmbed embed) => await message.ModifyAsync(embed);
        public async Task<DiscordMessage> EditMessage(DiscordMessage message, DiscordMessageBuilder builder) => await message.ModifyAsync(builder);
    }
}
