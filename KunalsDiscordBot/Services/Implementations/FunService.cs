using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.FunCommands;
using DSharpPlus.Entities;
using DSharpPlus;

namespace KunalsDiscordBot.Services.Fun
{
    public class FunService : IFunService
    { 
        private Dictionary<ulong, Spammer> spammers { get; set; } = new Dictionary<ulong, Spammer>();
        private Dictionary<int, GhostData> presences { get; set; } = new Dictionary<int, GhostData>();

        public async Task<GhostPresence> CreatePresence(ulong guildId, ulong memberId, DiscordClient client, DiscordDmChannel dm, DiscordChannel channel)
        {
            if (await GetPresence(guildId, memberId) != null)
                return null;

            var presence = new GhostPresence(client, dm, channel, guildId, memberId);

            var index = presences.Count;

            presences.Add(index, new GhostData { guildId = guildId, userID = memberId, presence = presence });
            presence.OnPresenceEnded.WithEvent(() => presences.Remove(index));
            await Task.Run(async() => await presence.BegineGhostPresence());

            return presence;
        }

        public Task<GhostData> GetPresence(ulong guildId, ulong memberId) => Task.FromResult(presences.Values.FirstOrDefault(x => x.guildId == guildId || x.userID == memberId));

        public async Task<Spammer> CreateSpammer(ulong guildId, string message, int timer, DiscordChannel channel)
        {
            if (spammers.ContainsKey(guildId))
                return null;

            var spammer = new Spammer(timer, message, channel);

            spammers.Add(guildId, spammer);
            spammer.OnSpamEnded.WithEvent(() => spammers.Remove(guildId));
            await Task.Run(async() =>  await spammer.Spam());

            return spammer;
        }

        public Task<bool> StopSpammer(ulong guildId)
        {
            if (!spammers.ContainsKey(guildId))
                return Task.FromResult(false);

            spammers[guildId].StopSpam();
            return Task.FromResult(true);
        }
    }
}
