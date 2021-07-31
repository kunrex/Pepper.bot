using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;
using DiscordBotDataBase.Dal.Models.Servers;

namespace KunalsDiscordBot.Services.General
{
    public class ServerService : IServerService
    {
        private DataContext context;

        public ServerService(DataContext _context) => context = _context;

        public async Task<Rule> GetRule(ulong guildId, int index)
        {
            if (index < 0)
                return null;

            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)guildId);
            if (profile == null)
                return null;

            var rules = context.ServerRules.AsQueryable().Where(x => x.ServerProfileId == profile.Id).ToList();

            return index >= rules.Count ? null : rules[index];
        }

        public async Task<bool> SetMuteRoleId(ulong id, ulong roleId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.MutedRoleId = (long)roleId;
            var updateEntry = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<ServerProfile> CreateServerProfile(ulong guildId)
        {
            var serverProfile = new ServerProfile { GuildId = (long)guildId };

            await context.AddAsync(serverProfile);
            await context.SaveChangesAsync();

            return serverProfile;
        }

        public async Task<bool> AddOrRemoveRule(ulong id, string ruleContent, bool add)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
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
                var rule = context.ServerRules.AsQueryable().FirstOrDefault(x => x.ServerProfileId == profile.Id && x.RuleContent == ruleContent);
                context.ServerRules.Remove(rule);
            }

            var updateEntry = context.ServerProfiles.Update(profile);
            await context.SaveChangesAsync();

            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        private Task<bool> CheckIfRuleExists(ulong id, string ruleContent)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            var rules = context.ServerRules.AsQueryable().Where(x => x.ServerProfileId == profile.Id).ToList();

            return Task.FromResult(rules.FirstOrDefault(x => x.RuleContent == ruleContent) != null);
        }

        public async Task<ServerProfile> GetServerProfile(ulong id)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                profile = await CreateServerProfile(id);

            return profile;
        }

        public async Task<bool> ToggleNSFW(ulong id, bool toToggle)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                return false;

            profile.AllowNSFW = toToggle ? 1 : 0 ;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> ToggleDJOnly(ulong id, bool toToggle)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                return false;

            profile.UseDJRoleEnforcement = toToggle ? 1 : 0;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> SetDJRole(ulong id, ulong roleID)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                return false;

            profile.DJRoleId = (long)roleID;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public Task<ulong> GetDJRole(ulong id) => Task.FromResult((ulong)(context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id)).DJRoleId);

        public async Task<bool> TogglePermissions(ulong id, bool toToggle)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                return false;

            profile.RestrictPermissionsToAdmin = toToggle ? 1 : 0;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<List<Rule>> GetAllRules(ulong guildId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)guildId);
            if (profile == null)
                profile = await CreateServerProfile(guildId);

            return context.ServerRules.AsQueryable().Where(x => x.ServerProfileId == profile.Id).ToList();
        }

        public async Task RemoveServerProfile(ulong id)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);

            if (profile == null)
                return;

            context.ServerProfiles.Remove(profile);
            await context.SaveChangesAsync();
        }

        public async Task<bool> ToggleLogErrors(ulong id, bool toToggle)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.LogErrors = toToggle ? 1 : 0;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> ToggleNewMemberLog(ulong id, bool toToggle)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.LogNewMembers = toToggle ? 1 : 0;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> SetLogChannel(ulong id, ulong channelId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.LogChannel = (long)channelId;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> SetRuleChannel(ulong id, ulong channelId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.RulesChannelId = (long)channelId;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }

        public async Task<bool> SetModeratorRole(ulong id, ulong roleId)
        {
            var profile = context.ServerProfiles.FirstOrDefault(x => x.GuildId == (long)id);
            if (profile == null)
                return false;

            profile.ModeratorRoleId = (long)roleId;
            var update = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            return true;
        }
    }
}
