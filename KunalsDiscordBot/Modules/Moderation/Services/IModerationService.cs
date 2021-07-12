using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Moderation;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

using System.Collections.Generic;

namespace KunalsDiscordBot.Modules.Moderation.Services
{
    public interface IModerationService
    {
        public Task<ModerationProfile> GetModerationProfile(ulong id, ulong guildId);
        public Task<ModerationProfile> GetModerationProfile(int id);
        public Task<ModerationProfile> CreateModerationProfile(ulong id, ulong guildId);

        public Task<int> AddInfraction(ulong id, ulong guildId, ulong moderatorID, string reason);
        public Task<int> AddEndorsement(ulong id, ulong guildId, ulong moderatorID, string reason);
        public Task<int> AddBan(ulong id, ulong guildId, ulong moderatorID, string reason, string time);
        public Task<int> AddKick(ulong id, ulong guildId, ulong moderatorID, string reason);
        public Task<int> AddMute(ulong id, ulong guildId, ulong moderatorID, string reason, string time);

        public Task<Infraction> GetInfraction(int infractionID);
        public Task<Endorsement> GetEndorsement(int endorsementID);
        public Task<Ban> GetBan(int banID);
        public Task<Kick> GetKick(int kickID);
        public Task<Mute> GetMute(int muteId);

        public Task<List<Infraction>> GetInfractions(ulong id, ulong guildId);
        public Task<List<Endorsement>> GetEndorsements(ulong id, ulong guildId);
        public Task<List<Ban>> GetBans(ulong id, ulong guildId);
        public Task<List<Kick>> GetKicks(ulong id, ulong guildId);
        public Task<List<Mute>> GetMutes(ulong id, ulong guildId);
        public Task<List<Mute>> GetMutes(ulong guildId);

        public Task<bool> ClearInfractions(ulong id, ulong guildId);
        public Task<bool> ClearEndorsements(ulong id, ulong guildId);

        public Task<bool> SetMuteRoleId(ulong id, ulong roleId);
        public Task<ulong> GetMuteRoleId(ulong id);
        public Task<bool> AddOrRemoveRule(ulong id, string ruleToAdd, bool add);
        public Task<Rule> GetRule(ulong guildId, int index);
        public Task<ServerProfile> CreateServerProfile(ulong guildId);
    }
}
