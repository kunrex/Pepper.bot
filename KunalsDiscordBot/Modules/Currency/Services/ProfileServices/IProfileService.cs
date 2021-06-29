using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;
using DSharpPlus.Entities;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;

namespace KunalsDiscordBot.Services.Currency
{
    public interface IProfileService
    {
        public Task<Profile> GetProfile(ulong id, string name, bool sameMember = true);
        public Task<Profile> CreateProfile(ulong id, string name);

        public Task<bool> AddXP(ulong id, int val);
        public Task<bool> ChangeCoins(ulong id, int val);
        public Task<bool> ChangeCoinsBank(ulong id, int val);
        public Task<bool> ChangeMaxCoinsBank(ulong id, int val);

        public Task<ItemDBData> GetItem(ulong id, string name);
        public Task<List<ItemDBData>> GetItems(ulong id);
        public Task<bool> AddOrRemoveItem(ulong id, string name, int quantity);

        public Task<bool> ChangeJob(ulong id, string jobName);

        public Task<BoostData> GetBoost(ulong id, string name);
        public Task<List<BoostData>> GetBoosts(ulong id);
        public Task<bool> AddOrRemoveBoost(ulong id, string name, int value, int time, string startTime, int quantity);

        public int GetLevel(Profile profile);
        public Task<int> GetLevel(ulong id);

        public Task<bool> ChangeLogDate(ulong id, int logType, DateTime date);
        public Task<bool> ToggleSafeMode(ulong id);
    }
}
