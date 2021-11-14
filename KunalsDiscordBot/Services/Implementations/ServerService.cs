using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;

namespace KunalsDiscordBot.Services.General
{
    public class ServerService : DatabaseService, IServerService
    {
        public ServerService(DataContext _context) : base(_context)
        {

        }

        public async Task<ServerProfile> GetServerProfile(ulong id)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                profile = await CreateServerProfile(id);

            return profile;
        }

        public async Task<ServerProfile> CreateServerProfile(ulong guildId)
        {
            var cached = (long)guildId;

            var serverProfile = new ServerProfile
            {
                Id = cached,
                ModerationData = new ModerationData { Id = cached },
                FunData = new FunData { Id = cached },
                MusicData = new MusicData { Id = cached },
                GameData = new GameData { Id = cached },
                ChatData = new AIChatData {  Id = cached },
            };

            await AddEntity(serverProfile);
            return serverProfile;
        }

        public async Task RemoveServerProfile(ulong id)
        {
            var cached = (long)id;
            var profile = context.ServerProfiles.FirstOrDefault(x => x.Id == cached);

            if (profile == null)
                return;

            await RemoveEntity(profile);
        }

        public Task<MusicData> GetMusicData(ulong guildId) => Task.FromResult(context.MusicDatas.FirstOrDefault(x => x.Id == (long)guildId));

        public Task<FunData> GetFunData(ulong guildId) => Task.FromResult(context.FunDatas.FirstOrDefault(x => x.Id == (long)guildId));

        public Task<ModerationData> GetModerationData(ulong guildId) => Task.FromResult(context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId));

        public Task<GameData> GetGameData(ulong guildId) => Task.FromResult(context.GameDatas.FirstOrDefault(x => x.Id == (long)guildId));

        public async Task<AIChatData> GetChatData(ulong guildId)
        {
            var chatData = context.ChatDatas.FirstOrDefault(x => x.Id == (long)guildId);
            if (chatData == null)
                await AddEntity(new AIChatData { Id = (long)guildId, ServerProfileId = (long)guildId });

            return context.ChatDatas.FirstOrDefault(x => x.Id == (long)guildId);
        }

        public async Task<bool> ModifyData<T>(T data, Action<T> modification) where T : IEntity => await ModifyEntity(data, modification);
    }
}
