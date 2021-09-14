using System;
using System.Threading.Tasks;

using DSharpPlus.Interactivity;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces
{
    public interface ITextInputCommunicator
    {
        public Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data);
    }
}
