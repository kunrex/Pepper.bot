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

        public async Task<Profile> GetProfile(ulong id, string name, bool defaultCreate = false)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null && defaultCreate)
                profile = await CreateProfile(id, name);

            await DetatchProfile(profile);
            return profile;
        }

        public async Task<Profile> CreateProfile(ulong id, string name)
        {
            var profile = new Profile
            {
                Id = (long)id,
                Name = name,
                XP = 0,
                Coins = 0,
                CoinsBank = 0,
                CoinsBankMax = 100,
                Job = "None",
                PrevWorkDate = "None",
                SafeMode = 0,
                Level = 1
            };

            var entityEntry = await context.UserProfiles.AddAsync(profile).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            entityEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return profile;
        }


        public async Task<bool> UpdateProfile(Profile profileToUpdate)
        {
            if (profileToUpdate == null)
                return false;

            var updateEntry = context.UserProfiles.Update(profileToUpdate);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public Task<bool> DetatchProfile(Profile profileToDetatch)
        {
            if (profileToDetatch == null)
                return Task.FromResult(false);

            context.Entry(profileToDetatch).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return Task.FromResult(true);
        }

        public async Task<bool> AddXP(ulong id, int val)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.XP += val;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> ChangeCoins(ulong id, int val)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.Coins += val;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }


        public async Task<bool> ChangeMaxCoinsBank(ulong id, int val)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.CoinsBankMax += val;

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> ChangeCoinsBank(ulong id, int val)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.CoinsBank += val;
            profile.CoinsBank = Math.Clamp(profile.CoinsBank, 0, profile.CoinsBankMax);

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
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
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            var item = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            if (item != null)//item already exists
            {
                item.Count += quantity;

                if(item.Count == 0)
                {
                    var removeEntry = context.ProfileItems.Remove(item);
                    await context.SaveChangesAsync();

                    removeEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    return true;
                }

                var _updateEntry = context.ProfileItems.Update(item);
                await context.SaveChangesAsync();

                _updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                return true;
            }

            item = new ItemDBData { Name = name, Count = quantity };
            profile.Items.Add(item);

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<bool> ChangeJob(ulong id, string jobName)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.Job = jobName;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> AddOrRemoveBoost(ulong id, string name, int value, TimeSpan time, string startTime, int quantity)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            var boost = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.BoosteName.ToLower() == name.ToLower());
            if (boost != null)//boost already exists
            {
                if(quantity > 0)
                    return false;//prevent more than one boost
                else 
                {
                    var removeEntry = context.ProfileBoosts.Remove(boost);
                    await context.SaveChangesAsync();

                    removeEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                    return true;
                }
            }

            boost = new BoostData { BoosteName = name, BoostTime = time.ToString(), BoostValue = value, BoostStartTime = startTime };
            profile.Boosts.Add(boost);

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
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

        public async Task<bool> ToggleSafeMode(ulong id)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.SafeMode = profile.SafeMode == 1 ? 0 : 1;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync().ConfigureAwait(false);

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> ChangePreviousWorkData(ulong id, DateTime date)
        {
            var profile = context.UserProfiles.FirstOrDefault(x => x.Id == (long)id);

            if (profile == null)
                return false;

            profile.PrevWorkDate = date.ToString("MM/dd/yyyy HH:mm:ss");
            var updateEntry = context.UserProfiles.Update(profile);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }
    }
}
