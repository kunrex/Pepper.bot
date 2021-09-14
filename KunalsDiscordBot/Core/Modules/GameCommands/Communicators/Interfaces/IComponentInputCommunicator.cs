using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators.Interfaces
{
    public interface IComponentInputCommunicator
    {
        public Task<string> Input(InteractivityExtension interactivity, DiscordMessage message, DiscordUser user, InputData data);
    }
}
