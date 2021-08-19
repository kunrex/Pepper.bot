using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.GameCommands.Players.Spectators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public interface ISpectatorGame
    {
        public abstract List<DiscordSpectator> spectators { get; set; }

        public Task<bool> AddSpectator(DiscordMember member);

        public Task<bool> RemoveSpectator(DiscordSpectator spectator)
        {
            spectators.Remove(spectator);
            return Task.FromResult(true);
        }
    }
}
