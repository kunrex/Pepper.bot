using System;
using System.Threading.Tasks;

using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;

namespace KunalsDiscordBot.Services.Moderation
{
    public interface IModerationService
    {
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

        public Task<bool> AddOrRemoveCustomCommand(ulong id, string commandTitle, bool add, string commandContent = "Unspecified");
        public Task<CustomCommand> GetCustomCommand(ulong guildId, string commandTitle);
        public Task<IEnumerable<CustomCommand>> GetAllCustomCommands(ulong guildId);

        public Task<bool> AddOrRemoveFilteredWord(ulong id, string word, bool addInfraction, bool add);
        public Task<FilteredWord> GetFilteredWord(ulong guildId, string word);
        public Task<IEnumerable<FilteredWord>> GetAllFilteredWords(ulong guildId);

        public Task<bool> ClearInfractions(ulong id, ulong guildId);
        public Task<bool> ClearEndorsements(ulong id, ulong guildId);

        public Task<bool> ClearAllServerModerationData(ulong serverId);
    }
}
