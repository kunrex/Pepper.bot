using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using System.Linq;
using DSharpPlus.Entities;
using DiscordBotDataBase.Dal.Models.Profile;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Items;

namespace KunalsDiscordBot.Services.Currency
{
    public class ProfileService : BotService, IProfileService
    {
        private readonly DataContext context;

        public ProfileService(DataContext _context) => context = _context;

        public async Task<Profile> GetProfile(DiscordMember member, bool sameMember = true)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)member.Id);

            if (profile == null && sameMember)
                profile = await CreateProfile(member);

            return profile;
        }

        public async Task<Profile> CreateProfile(DiscordMember member)
        {
            var profile = new Profile
            {
                DiscordUserID = (long)member.Id,
                Name = member.Username,
                XP = 0,
                Coins = 0,
                CoinsBank = 0
            };

            await context.UserProfiles.AddAsync(profile).ConfigureAwait(false);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return profile;
        }

        public async Task<bool> AddXP(DiscordMember member, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)member.Id);

            if (profile == null)
                return false;

            profile.XP += val;
            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeCoins(DiscordMember member, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)member.Id);

            if (profile == null)
                return false;

            profile.Coins += val;
            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangeCoinsBank(DiscordMember member, int val)
        {
            var profile = await context.UserProfiles.FirstOrDefaultAsync(x => x.DiscordUserID == (long)member.Id);

            if (profile == null)
                return false;

            profile.CoinsBank += val;
            context.UserProfiles.Update(profile);
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<ItemDBData> GetItem(DiscordMember member, string name)
        {
            return null;
        }

        public async Task<List<ItemDBData>> GetItems(DiscordMember member)
        {
            return null;
        }

        public async Task<bool> AddItem(DiscordMember member, string name, int quantity)
        {
            return true;
        }
    }
}
