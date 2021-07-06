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
                var span = TimeSpan.FromHours(boost.BoostTime);

                if(DateTime.Now - startTime > span)
                {
                    var profile = context.UserProfiles.First(x => x.Id == boost.ProfileId);
                    await AddOrRemoveBoost((ulong)profile.DiscordUserID, boost.BoosteName, 0, 0, "", -1);
                }
            }
        }

        public async Task<Profile> GetProfile(ulong id, string name, bool sameMember = true)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null && sameMember)
                profile = await CreateProfile(id, name);

            return profile;
        }

        public async Task<Profile> CreateProfile(ulong id, string name)
        {
            var profile = new Profile
            {
                DiscordUserID = (long)id,
                Name = name,
                XP = 0,
                Coins = 0,
                CoinsBank = 0,
                CoinsBankMax = 100,
                Job = "None",
                PrevLogDate = "None",
                PrevWeeklyLogDate = "None",
                PrevMonthlyLogDate = "None",
                PrevWorkDate = "None",
                SafeMode = 0
            };

            var entityEntry = await context.UserProfiles.AddAsync(profile).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);
            entityEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return profile;
        }

        public async Task<bool> AddXP(ulong id, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;

            profile.XP += val;
            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeCoins(ulong id, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

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
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

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
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;

            profile.CoinsBank += val;
            profile.CoinsBank = System.Math.Clamp(profile.CoinsBank, 0, profile.CoinsBankMax);

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<ItemDBData> GetItem(ulong id, string name)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return null;

            var item = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

            return item;
        }

        public async Task<List<ItemDBData>> GetItems(ulong id)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);
            var items = context.ProfileItems.AsQueryable().Where(x => x.ProfileId == profile.Id).ToList();

            return items;
        }

        public async Task<bool> AddOrRemoveItem(ulong id, string name, int quantity)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

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
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;

            profile.Job = jobName;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> AddOrRemoveBoost(ulong id, string name, int value, int time, string startTime, int quantity)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

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

            boost = new BoostData { BoosteName = name, BoostTime = time, BoostValue = value, BoostStartTime = startTime };
            profile.Boosts.Add(boost);

            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<BoostData> GetBoost(ulong id, string name)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return null;

            var boost = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).FirstOrDefault(x => x.BoosteName.ToLower() == name.ToLower());

            return boost;
        }

        public async Task<List<BoostData>> GetBoosts(ulong id)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);
            var boosts = context.ProfileBoosts.AsQueryable().Where(x => x.ProfileId == profile.Id).ToList();

            return boosts;
        }

        public int GetLevel(Profile profile) => (int)(MathF.Floor(25 + MathF.Sqrt(625 + 100 * profile.XP)) / 50);

        public async Task<int> GetLevel(ulong id)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return -1;

            return (int)(MathF.Floor(25 + MathF.Sqrt(625 + 100 * profile.XP)) / 50);
        }

        public async Task<bool> ChangeLogDate(ulong id, int index, DateTime date)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;
            switch(index)
            {
                case 0:
                    profile.PrevLogDate = date.ToString("MM/dd/yyyy HH:mm:ss");
                    break;
                case 1:
                    profile.PrevWeeklyLogDate = date.ToString("MM/dd/yyyy HH:mm:ss");
                    break;
                case 2:
                    profile.PrevMonthlyLogDate = date.ToString("MM/dd/yyyy HH:mm:ss");
                    break;
                case 3:
                    profile.PrevWorkDate = date.ToString("MM/dd/yyyy HH:mm:ss");
                    break;
            }

            var updateEntry = context.UserProfiles.Update(profile);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<bool> ToggleSafeMode(ulong id)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;

            profile.SafeMode = profile.SafeMode == 1 ? 0 : 1;
            var updateEntry = context.UserProfiles.Update(profile);
            await context.SaveChangesAsync().ConfigureAwait(false);

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }
    }
}
