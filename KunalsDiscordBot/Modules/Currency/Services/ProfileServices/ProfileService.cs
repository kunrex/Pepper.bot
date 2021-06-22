//System name spaces
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;

using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;

namespace KunalsDiscordBot.Services.Currency
{
    public class ProfileService : BotService, IProfileService
    {
        private readonly DataContext context;

        public ProfileService(DataContext _context) => context = _context;

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
                Job = "None"
            };

            await context.UserProfiles.AddAsync(profile).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

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

        public async Task<bool> ChangeCoinsBank(ulong id, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)id);

            if (profile == null)
                return false;

            if (profile.CoinsBankMax == 0)
                profile.CoinsBankMax = 100;

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
    }
}
