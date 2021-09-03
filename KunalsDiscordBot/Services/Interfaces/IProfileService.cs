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

        public Task<bool> RemoveEntity<T>(T entityToRemove);
        public Task<bool> AddEntity<T>(T entityToAdd);
        public Task<bool> UpdateEntity<T>(T entityToUpdate);

        public Task<bool> ModifyProfile(Profile profileToModify, Action<Profile> modification);
        public Task<bool> ModifyProfile(ulong id, Action<Profile> modification);

        public Task<ItemDBData> GetItem(ulong id, string name);
        public Task<List<ItemDBData>> GetItems(ulong id);
        public Task<bool> AddOrRemoveItem(ulong id, string name, int quantity);

        public Task<BoostData> GetBoost(ulong id, string name);
        public Task<List<BoostData>> GetBoosts(ulong id);
        public Task<bool> AddOrRemoveBoost(ulong id, string name, int value, TimeSpan time, string startTime, int quantity);
    }
}
