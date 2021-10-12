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

        public Task<bool> ModifyData<T>(T data, Action<T> modification) where T : Entity<long>;
 
        public Task<bool> AddOrRemoveRule(ulong id, string ruleToAdd, bool add);
        public Task<Rule> GetRule(ulong guildId, int index);
        public Task<List<Rule>> GetAllRules(ulong guildId);

        public Task<MusicData> GetMusicData(ulong guildId);
        public Task<FunData> GetFunData(ulong guildId);
        public Task<ModerationData> GetModerationData(ulong guildId);
        public Task<GameData> GetGameData(ulong guildId);
        public Task<AIChatData> GetChatData(ulong guildId);
    }
}
