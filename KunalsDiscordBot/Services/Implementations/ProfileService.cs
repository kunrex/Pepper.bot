﻿//System name spaces
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;

namespace KunalsDiscordBot.Services.Currency
{
    public class ProfileService : DatabaseService, IProfileService
    {
        public ProfileService(DataContext _context) : base(_context)
        {
            
        }

        public async Task<Profile> GetProfile(ulong id, string name, bool defaultCreate = false)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null && defaultCreate)
                profile = await CreateProfile(id, name);

            return profile;
        }

        private async Task<Profile> CreateProfile(ulong id, string name)
        {
            var profile = new Profile
            {
                Id = (long)id,
                Name = name,

                Level = 1,
                XP = 0,

                Coins = 0,
                CoinsBank = 0,
                CoinsBankMax = 100,

                Job = "None",
                PrevWorkDate = "None",

                SafeMode = 0,
            };

            await AddEntity(profile);

            return profile;
        }

        public async Task<bool> ModifyProfile(Profile profileToModify, Action<Profile> modification) => await ModifyEntity(profileToModify, modification);

        public async Task<bool> ModifyProfile(ulong id, Action<Profile> modification)
        {
            var profileToModify = await GetProfile(id, "");

            return await ModifyProfile(profileToModify, modification);
        }

        public async Task<ItemDBData> GetItem(ulong id, string name)
        {
            var items = await GetItems(id);
            if (items == null)
                return null;

            return items.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }

        public Task<IEnumerable<ItemDBData>> GetItems(ulong id)
        {
            long casted = (long)id;

            return Task.FromResult(context.ProfileItems.AsEnumerable().Where(x => x.ProfileId == casted));
        }

        public async Task<bool> AddOrRemoveItem(ulong id, string name, int quantity)
        {
            var profile = await GetProfile(id, "");
            if (profile == null)
                return false;

            return await AddOrRemoveItem(profile, name, quantity);
        }

        public async Task<bool> AddOrRemoveItem(Profile profile, string name, int quantity)
        {
            var item = await GetItem((ulong)profile.Id, name);

            if (item != null)//item already exists
            {
                item.Count += quantity;

                if (item.Count == 0)
                    return await RemoveEntity(item);

                return await UpdateEntity(item);
            }

            item = new ItemDBData { ProfileId = profile.Id, Name = name, Count = quantity };
            profile.Items.Add(item);

            return await UpdateEntity(profile);
        }

        public async Task<Boost> GetBoost(ulong id, string name)
        {
            var boosts = await GetBoosts(id);
            if (boosts == null)
                return null;

            return boosts.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task<IEnumerable<Boost>> GetBoosts(ulong id)
        {
            var casted = (long)id;

            var boosts = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == casted).ToList();

            foreach (var boost in boosts)
            {
                var startTime = DateTime.Parse(boost.StartTime);
                var span = TimeSpan.Parse(boost.TimeSpan);

                if (DateTime.Now - startTime > span)
                {
                    await RemoveEntity(boost);
                    boosts.Remove(boost);
                }
            }

            return boosts.Select(x => (Boost)x);
        }

        public async Task<bool> AddOrRemoveBoost(ulong id, string name, int value, TimeSpan time, string startTime, int quantity)
        {
            var profile = await GetProfile(id, "");
            if (profile == null)
                return false;

            return await AddOrRemoveBoost(profile, name, value, time, startTime, quantity);
        }

        public async Task<bool> AddOrRemoveBoost(Profile profile, string name, int value, TimeSpan time, string startTime, int quantity)
        {
            var boost = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (boost != null)//boost already exists
            {
                if (quantity > 0)
                    return false;//prevent more than one boost
                else
                    return !await RemoveEntity(boost);
            }

            boost = new BoostData { ProfileId = profile.Id, Name = name, TimeSpan = time.ToString(), PercentageIncrease = value, StartTime = startTime };
            profile.Boosts.Add(boost);

            return await UpdateEntity(profile);
        }

        public async Task<bool> KillProfile(ulong id, bool checkInvincibilityBoots = true)
        {
            var profile = await GetProfile(id, "", false);

            return await KillProfile(profile, checkInvincibilityBoots);
        }

        public async Task<bool> KillProfile(Profile profile, bool checkInvincibilityBoots = true)
        {
            var casted = (ulong)profile.Id;
            var boosts = await GetBoosts(casted);

            if (checkInvincibilityBoots && boosts.FirstOrDefault(x => x is InvincibilityBoost) != null)
                return false;
            
            await ModifyProfile(profile, async(x) =>
            {
                x.Coins = 0;
                foreach (var boost in boosts)
                    await AddOrRemoveBoost(casted, boost.Name, 0, TimeSpan.FromSeconds(0), "", -1);
            });

            return true;
        }
    }
}
