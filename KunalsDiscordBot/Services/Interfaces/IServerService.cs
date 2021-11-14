using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;


namespace KunalsDiscordBot.Services.General
{
    public interface IServerService
    {
        public Task<ServerProfile> GetServerProfile(ulong id);
        public Task<ServerProfile> CreateServerProfile(ulong id);
        public Task RemoveServerProfile(ulong id);

        public Task<bool> ModifyData<T>(T data, Action<T> modification) where T : IEntity;

        public Task<MusicData> GetMusicData(ulong guildId);
        public Task<FunData> GetFunData(ulong guildId);
        public Task<ModerationData> GetModerationData(ulong guildId);
        public Task<GameData> GetGameData(ulong guildId);
        public Task<AIChatData> GetChatData(ulong guildId);
    }
}
