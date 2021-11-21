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
        public TicTacToeCommunicator() 
        {
 
        }

        public async Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData inputData)
        {
            if (inputData.InputType == InputType.Message || inputData.InputType == InputType.Dropdown)
                throw new InvalidOperationException();

            var result = await WaitForButton(interactivity, message, user, inputData.Span);

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
                    var emoji = state == 0 ? TicTacToe.blank : (state == 1 ? TicTacToe.x : TicTacToe.o);

                    var button = emoji == TicTacToe.blank ? new DiscordButtonComponent(ButtonStyle.Primary, returnValue, "-", disableAll)
                     : new DiscordButtonComponent(ButtonStyle.Primary, returnValue, "", true, new DiscordComponentEmoji(DiscordEmoji.FromName(client, emoji)));
                    buttons.Add(button);

                    if ((index + 1) % 3 == 0)
                    {
                        builder.AddComponents(buttons);

                        buttons = new List<DiscordButtonComponent>();
                    }
                    index++;
                }

            builder.AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, quitInputvalue, "Leave", disableAll));

            return await message.ModifyAsync(builder).ConfigureAwait(false);
        }
    }
}
