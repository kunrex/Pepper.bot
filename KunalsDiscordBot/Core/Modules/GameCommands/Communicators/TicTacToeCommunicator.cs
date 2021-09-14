using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class TicTacToeCommunicator : DiscordCommunicator, IComponentInputCommunicator
    {
        public DiscordChannel channel { get; private set; }

        public TicTacToeCommunicator(Regex expression, TimeSpan span, DiscordChannel _channel) : base(expression, span)
        {
            channel = _channel;
        }

        public async Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData data)
        {
            if (data.InputType == InputType.Message || data.InputType == InputType.Dropdown)
                throw new InvalidOperationException();

            var result = await WaitForButton(interactivity, message, user, data.Span);

            return result.TimedOut ? afkInputvalue : result.Result.Id;
        }

        public async Task<DiscordMessage> SendMessage(DiscordMessage message, string content) => await ModifyMessage(message, content);

        public async Task<DiscordMessage> PrintCompleteBoard(DiscordClient client, DiscordMessage message, string content, int[,] board, bool disableAll = false)
        {
            var builder = new DiscordMessageBuilder().WithContent(content);
            List<DiscordButtonComponent> buttons = new List<DiscordButtonComponent>();

            int index = 0;
            for (int i = 0; i < board.GetLength(0); i++)
                for (int k = 0; k < board.GetLength(1); k++)
                {
                    var state = board[i, k];
                    var returnValue = $"{i},{k}";
                    var emoji = state == 0 ? TicTacToe.Blank : (state == 1 ? TicTacToe.X : TicTacToe.O);

                    var button = emoji == TicTacToe.Blank ? new DiscordButtonComponent(ButtonStyle.Primary, returnValue, "-", disableAll)
                     : new DiscordButtonComponent(ButtonStyle.Primary, returnValue, "", true, new DiscordComponentEmoji(DiscordEmoji.FromName(client, emoji)));
                    buttons.Add(button);

                    if ((index + 1) % 3 == 0)
                    {
                        builder.AddComponents(buttons);

                        buttons = new List<DiscordButtonComponent>();
                    }
                    index++;
                }

            builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, DiscordCommunicator.quitInputvalue, "Leave", disableAll));

            return await message.ModifyAsync(builder).ConfigureAwait(false);
        }
    }
}
