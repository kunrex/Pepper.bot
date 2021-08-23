using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Modules.FunCommands;

namespace KunalsDiscordBot.Services.Fun
{
    public class FunService : IFunService
    { 
        private Dictionary<ulong, Spammer> Spammers { get; set; } = new Dictionary<ulong, Spammer>();
        private Dictionary<int, GhostData> Presences { get; set; } = new Dictionary<int, GhostData>();

        public async Task<GhostPresence> CreatePresence(ulong guildId, ulong memberId, DiscordClient client, DiscordDmChannel dm, DiscordChannel channel)
        {
            if (await GetPresence(guildId, memberId) != null)
                return null;

            var presence = new GhostPresence(client, dm, channel, guildId, memberId);

            var index = Presences.Count;

            Presences.Add(index, new GhostData { guildId = guildId, userID = memberId, presence = presence });
            presence.OnPresenceEnded.WithEvent(() => Presences.Remove(index));
            await Task.Run(async() => await presence.BegineGhostPresence());

            return presence;
        }

        public Task<GhostData> GetPresence(ulong guildId, ulong memberId) => Task.FromResult(Presences.Values.FirstOrDefault(x => x.guildId == guildId || x.userID == memberId));

        public async Task<Spammer> CreateSpammer(ulong guildId, string message, int timer, DiscordChannel channel)
        {
            if (Spammers.ContainsKey(guildId))
                return null;

            var spammer = new Spammer(timer, message, channel);

            Spammers.Add(guildId, spammer);
            spammer.OnSpamEnded.WithEvent(() => Spammers.Remove(guildId));
            await Task.Run(async() =>  await spammer.Spam());

            return spammer;
        }

        public Task<bool> StopSpammer(ulong guildId)
        {
            if (!Spammers.ContainsKey(guildId))
                return Task.FromResult(false);

            Spammers[guildId].StopSpam();
            return Task.FromResult(true);
        }
    }
}
