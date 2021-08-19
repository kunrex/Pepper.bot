using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Modules.FunCommands;

namespace KunalsDiscordBot.Services.Fun
{
    public interface IFunService
    {
        public Task<Spammer> CreateSpammer(ulong guildId, string message, int timer, DiscordChannel channel);
        public Task<bool> StopSpammer(ulong guildId);

        public Task<GhostPresence> CreatePresence(ulong guildId, ulong memberId, DiscordClient client, DiscordDmChannel dm, DiscordChannel channel);
        public Task<GhostData> GetPresence(ulong guildId, ulong memberId);
    }
}
