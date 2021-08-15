using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Services.General
{
    public interface IServerService
    {
        public Task<ServerProfile> GetServerProfile(ulong id);
        public Task<ServerProfile> CreateServerProfile(ulong id);
        public Task RemoveServerProfile(ulong id);

        public Task<bool> ToggleNSFW(ulong id, bool toToggle);
        public Task<bool> ToggleSpamCommand(ulong id, bool toToggle);
        public Task<bool> ToggleGhostCommand(ulong id, bool toToggle);

        public Task<bool> SetConnect4Channel(ulong id, ulong toSet);
        public Task<bool> SetTicTacToeChannel(ulong id, ulong toSet);

        public Task<bool> ToggleDJOnly(ulong id, bool toToggle);
        public Task<bool> SetDJRole(ulong id, ulong roleID);
        public Task<ulong> GetDJRole(ulong id);

        public Task<bool> TogglePermissions(ulong id, bool toToggle);

        public Task<bool> SetMuteRoleId(ulong id, ulong roleId);
        public Task<bool> AddOrRemoveRule(ulong id, string ruleToAdd, bool add);
        public Task<Rule> GetRule(ulong guildId, int index);

        public Task<bool> ToggleLogErrors(ulong id, bool toToggle);
        public Task<bool> ToggleNewMemberLog(ulong id, bool toToggle);
        public Task<bool> SetWelcomeChannel(ulong id, ulong channelId);
        public Task<bool> SetRuleChannel(ulong id, ulong channelId);

        public Task<bool> SetModeratorRole(ulong id, ulong roleId);

        public Task<List<Rule>> GetAllRules(ulong guildId);
        public Task<MusicData> GetMusicData(ulong guildId);
        public Task<FunData> GetFunData(ulong guildId);
        public Task<ModerationData> GetModerationData(ulong guildId);
        public Task<GameData> GetGameData(ulong guildId);
    }
}
