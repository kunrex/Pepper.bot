//System name spaces
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;

namespace KunalsDiscordBot.Services.Currency
{
    public class ProfileService : BotService, IProfileService
    {
        private readonly DataContext context;

        public ProfileService(DataContext _context)
        {
            context = _context;

            CheckBoosts();
        }

        private async void CheckBoosts()
        {
            foreach (var boost in context.ProfileBoosts)
            {
                var startTime = DateTime.Parse(boost.BoostStartTime);
                var span = TimeSpan.Parse(boost.BoostTime);

                if(DateTime.Now - startTime > span)
                {
                    var profile = context.UserProfiles.First(x => x.Id == boost.ProfileId);
                    await AddOrRemoveBoost((ulong)profile.Id, boost.BoosteName, 0, TimeSpan.FromSeconds(0), "", -1);
                }
            }
        }

        public async Task<bool> RemoveEntity<T>(T entityToRemove)
        {
            var removeEntry = context.Remove(entityToRemove);
            await context.SaveChangesAsync();

            removeEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<bool> AddEntity<T>(T entityToAdd)
        {
            var entityEntry = await context.AddAsync(entityToAdd).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            entityEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
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

            var entityEntry = await context.UserProfiles.AddAsync(profile).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            entityEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return profile;
        }

        public async Task<bool> UpdateEntity<T>(T entityToUpdate)
        {
            if (entityToUpdate == null)
                return false;

            var updateEntry = context.Update(entityToUpdate);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<bool> ModifyProfile(Profile profileToModify, Action<Profile> modification)
        {
            modification.Invoke(profileToModify);

            return await UpdateEntity(profileToModify);
        }

        public async Task<bool> ModifyProfile(ulong id, Action<Profile> modification)
        {
            var profileToModify = await GetProfile(id, "");
            modification.Invoke(profileToModify);

            return await UpdateEntity(profileToModify);
        }

        public Task<ItemDBData> GetItem(ulong id, string name)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return null;

            var item = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

            return Task.FromResult(item);
        }

        public Task<List<ItemDBData>> GetItems(ulong id)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);
            if (profile == null)
                return null;

            var items = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).ToList();

            return Task.FromResult(items);
        }

        public async Task<bool> AddOrRemoveItem(ulong id, string name, int quantity)
        {
            var profile = await GetProfile(id, "");

            if (profile == null)
                return false;

            var item = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (item != null)//item already exists
            {
                item.Count += quantity;

                if(item.Count == 0)
                    return await RemoveEntity(item);

                return await UpdateEntity(item);
            }

            item = new ItemDBData { Name = name, Count = quantity };
            profile.Items.Add(item);

            return await UpdateEntity(profile);
        }

        public async Task<bool> AddOrRemoveBoost(ulong id, string name, int value, TimeSpan time, string startTime, int quantity)
        {
            var profile = await GetProfile(id, "");

            if (profile == null)
                return false;

            var boost = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.BoosteName.ToLower() == name.ToLower());
            if (boost != null)//boost already exists
            {
                if (quantity > 0)
                    return false;//prevent more than one boost
                else
                    return await RemoveEntity(boost);
            }

            boost = new BoostData { BoosteName = name, BoostTime = time.ToString(), BoostValue = value, BoostStartTime = startTime };
            profile.Boosts.Add(boost);

            return await UpdateEntity(profile);
        }

        public Task<BoostData> GetBoost(ulong id, string name)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return null;

            var boost = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.BoosteName.ToLower() == name.ToLower());

            return Task.FromResult(boost);
        }

        public Task<List<BoostData>> GetBoosts(ulong id)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);
            var boosts = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).ToList();

            return Task.FromResult(boosts);
        }
    }
}
