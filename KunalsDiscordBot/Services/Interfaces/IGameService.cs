using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.GameCommands;

namespace KunalsDiscordBot.Services.Games
{
    public interface IGameService
    {
        public Task<Game> StartGame<T>(ulong guildid, List<DiscordMember> players, DiscordClient client) where T : Game;
        public Task<Game> StartGame<T>(ulong guildid, List<DiscordMember> players, DiscordClient client, DiscordChannel channel, int cellCount) where T : Game;

        public Task<bool> AddSpectator<T>(ulong id, DiscordMember specator) where T : ISpectatorGame;

        public Task<Game> GetServerGame<T>(ulong id) where T : Game;
        public Task<Game> GetDMGame<T>(ulong playerId) where T : Game;
    }
}
