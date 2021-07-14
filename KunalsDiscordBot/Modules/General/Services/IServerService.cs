using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;
using DiscordBotDataBase.Dal.Models.Servers;

namespace KunalsDiscordBot.Services.General
{
    public interface IServerService
    {
        public Task<ServerProfile> GetServerProfile(ulong id);
        public Task<ServerProfile> CreateServerProfile(ulong id);

        public Task<bool> ToggleNSFW(ulong id, bool toToggle);

        public Task<bool> ToggleDJOnly(ulong id, bool toToggle);
        public Task<bool> SetDJRole(ulong id, ulong roleID);
        public Task<ulong> GetDJRole(ulong id);

        public Task<bool> TogglePermissions(ulong id, bool toToggle);

        public Task<bool> SetMuteRoleId(ulong id, ulong roleId);
        public Task<ulong> GetMuteRoleId(ulong id);
        public Task<bool> AddOrRemoveRule(ulong id, string ruleToAdd, bool add);
        public Task<Rule> GetRule(ulong guildId, int index);

        public Task<List<Rule>> GetAllRules(ulong guildId);
    }
}
