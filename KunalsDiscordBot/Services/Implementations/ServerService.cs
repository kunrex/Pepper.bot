using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;
using KunalsDiscordBot.Core.Attributes;

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
                ChatData = new AIChatData {  Id = cached }
            };

            await context.AddAsync(serverProfile);
            await context.SaveChangesAsync();

            return serverProfile;
        }


        public async Task RemoveServerProfile(ulong id)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return;

            context.ServerProfiles.Remove(profile);
            await context.SaveChangesAsync();
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

        public Task<Rule> GetRule(ulong guildId, int index)
        {
            if (index < 0)
                return null;

            var rules = context.ServerRules.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).ToList();

            return index >= rules.Count ? null : Task.FromResult(rules[index]);
        }

        public async Task<bool> AddOrRemoveRule(ulong id, string ruleContent, bool add)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)id);
            if (profile == null)
                return false;

            if (add)
            {
                if (await CheckIfRuleExists(id, ruleContent))
                    return false;

                profile.Rules.Add(new Rule
                {
                    RuleContent = ruleContent,
                });
            }
            else
            {
                var rule = context.ServerRules.AsQueryable().FirstOrDefault(x => x.ModerationDataId == profile.Id && x.RuleContent == ruleContent);
                context.ServerRules.Remove(rule);
            }

            var updateEntry = context.ModerationDatas.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        private Task<bool> CheckIfRuleExists(ulong id, string ruleContent)
        {
            var rules = context.ServerRules.AsQueryable().Where(x => x.ModerationDataId == (long)id).ToList();

            return Task.FromResult(rules.FirstOrDefault(x => x.RuleContent == ruleContent) != null);
        }

        public async Task<List<Rule>> GetAllRules(ulong guildId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.Id == (long)guildId);
            if (profile == null)
                profile = await CreateServerProfile(guildId);

            return context.ServerRules.AsQueryable().Where(x => x.ModerationDataId == profile.Id).ToList();
        }

        public async Task<bool> ModifyData<T>(T data, Action<T> modification) where T : Entity<long> => await ModifyEntity(data, modification);
    }
}
