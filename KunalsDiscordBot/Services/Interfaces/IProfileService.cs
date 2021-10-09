using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;

namespace KunalsDiscordBot.Services.Currency
{
    public interface IProfileService
    {
        public Task<Profile> GetProfile(ulong id, string name, bool sameMember = true);

        public Task<bool> ModifyProfile(Profile profileToModify, Action<Profile> modification);
        public Task<bool> ModifyProfile(ulong id, Action<Profile> modification);

        public Task<ItemDBData> GetItem(ulong id, string name);
        public Task<List<ItemDBData>> GetItems(ulong id);
        public Task<bool> AddOrRemoveItem(ulong id, string name, int quantity);
        public Task<bool> AddOrRemoveItem(Profile profile, string name, int quantity);

        public Task<Boost> GetBoost(ulong id, string name);
        public Task<List<Boost>> GetBoosts(ulong id);
        public Task<bool> AddOrRemoveBoost(ulong id, string name, int value, TimeSpan time, string startTime, int quantity);
        public Task<bool> AddOrRemoveBoost(Profile profile, string name, int value, TimeSpan time, string startTime, int quantity);

        public Task<bool> KillProfile(ulong id);
        public Task<bool> KillProfile(Profile profile);
    }
}
