using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Interactivity;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces
{
    public interface ITextInputCommunicator
    {
        public Regex InputExpression { get; }

        public Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData inputData);
    }
}
