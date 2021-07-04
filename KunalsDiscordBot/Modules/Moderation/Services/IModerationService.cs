using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Moderation;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

using System.Collections.Generic;

namespace KunalsDiscordBot.Modules.Moderation.Services
{
    public interface IModerationService
    {
        public Task<ModerationProfile> GetProfile(ulong id, ulong guildId);
        public Task<ModerationProfile> GetProfile(int id);
        public Task<ModerationProfile> CreateProfile(ulong id, ulong guildId);

        public Task<int> AddInfraction(ulong id, ulong guildId, ulong moderatorID, string reason);
        public Task<int> AddEndorsement(ulong id, ulong guildId, ulong moderatorID, string reason);
        public Task<int> AddBan(ulong id, ulong guildId, ulong moderatorID, string reason, string time);
        public Task<int> AddKick(ulong id, ulong guildId, ulong moderatorID, string reason);

        public Task<Infraction> GetInfraction(int infractionID);
        public Task<Endorsement> GetEndorsement(int endorsementID);
        public Task<Ban> GetBan(int banID);
        public Task<Kick> GetKick(int kickID);

        public Task<int> GetInfractions(ulong id, ulong guildId);
        public Task<int> GetEndorsements(ulong id, ulong guildId);
        public Task<int> GetBans(ulong id, ulong guildId);
        public Task<int> GetKicks(ulong id, ulong guildId);
    }
}
