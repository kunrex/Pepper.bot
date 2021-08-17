using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Players.Spectators;
using System.Linq;

namespace KunalsDiscordBot.Modules.Games
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
