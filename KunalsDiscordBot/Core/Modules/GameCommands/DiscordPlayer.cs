using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Players
{
    public abstract class DiscordPlayer<Communicator> where Communicator : DiscordCommunicator 
    {
        public DiscordPlayer(DiscordMember _member)
        {
            member = _member;
        }

        public DiscordMember member { get; protected set; }
        public Communicator communicator { get; protected set; }

        public abstract Task<bool> Ready(DiscordChannel channel);
    }
}
