using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Moderation;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

using System.Linq;
using System.Collections.Generic;

namespace KunalsDiscordBot.Modules.Moderation.Services
{
    public class ModerationService : IModerationService
    {
        private readonly DataContext context;

        public ModerationService(DataContext _context) => context = _context;

        public async Task<int> AddBan(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var ban = new Ban { Reason = reason, Time = time , ModeratorID = (long)moderatorID};
            profile.Bans.Add(ban);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return ban.Id;
        }

        public async Task<int> AddEndorsement(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var endorsement = new Endorsement { Reason = reason, ModeratorID = (long)moderatorID };
            profile.Endorsements.Add(endorsement);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return endorsement.Id;
        }

        public async Task<int> AddInfraction(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var infraction = new Infraction { Reason = reason, ModeratorID = (long)moderatorID };
            profile.Infractions.Add(infraction);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return infraction.Id;
        }

        public async Task<int> AddKick(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var kick = new Kick { Reason = reason, ModeratorID = (long)moderatorID };
            profile.Kicks.Add(kick);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return kick.Id;
        }

        public async Task<bool> AddOrRemoveRule(ulong id, string ruleContent, bool add)
        {
            var profile = await context.ServerProfiles.FirstOrDefaultAsync(x => x.GuildId == (long)id);
            if (profile == null)
                profile = await CreateServerProfile(id);

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

        private async Task<bool> CheckIfRuleExists(ulong id, string ruleContent)
        {
            var profile = await context.ServerProfiles.FirstOrDefaultAsync(x => x.GuildId == (long)id);
            var rules = context.ServerRules.AsQueryable().Where(x => x.ServerProfileId == profile.Id).ToList();

            return rules.FirstOrDefault(x => x.RuleContent == ruleContent) != null;
        }

        public async Task<bool> ClearEndorsements(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);
            if (profile == null)
                return false;

            var endorsements = context.ModEndorsements.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            if (endorsements.Count == 0)
                return false;

            foreach (var endorse in endorsements)
            {
                var updateEntry = context.ModEndorsements.Remove(endorse);
                await context.SaveChangesAsync();

                updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            return true;
        }

        public async Task<bool> ClearInfractions(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);
            if (profile == null)
                return false;

            var infractions = context.ModInfractions.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            if (infractions.Count == 0)
                return false;

            foreach (var infraction in infractions)
            {
                var updateEntry = context.ModInfractions.Remove(infraction);
                await context.SaveChangesAsync();

                updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            return true;
        }

        public async Task<ModerationProfile> CreateModerationProfile(ulong id, ulong guildId)
        {
            var modProfile = new ModerationProfile { DiscordId = (long)id, GuildId = (long)guildId };

            await context.AddAsync(modProfile);
            await context.SaveChangesAsync();

            return modProfile;
        }

        public async Task<ServerProfile> CreateServerProfile(ulong guildId)
        {
            var serverProfile = new ServerProfile { GuildId = (long)guildId };

            await context.AddAsync(serverProfile);
            await context.SaveChangesAsync();

            return serverProfile;
        }

        public async Task<Ban> GetBan(int banID) => await context.ModBans.FirstOrDefaultAsync(x => x.Id == banID);

        public async Task<int> GetBans(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return 0;
            }

            var bans = context.ModBans.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans.Count;
        }

        public async Task<Endorsement> GetEndorsement(int endorsementID) => await context.ModEndorsements.FirstOrDefaultAsync(x => x.Id == endorsementID);

        public async Task<int> GetEndorsements(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return 0;
            }

            var bans = context.ModEndorsements.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans.Count;
        }

        public async Task<Infraction> GetInfraction(int infractionID) => await context.ModInfractions.FirstOrDefaultAsync(x => x.Id == infractionID);

        public async Task<int> GetInfractions(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return 0;
            }

            var bans = context.ModInfractions.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans.Count;
        }

        public async Task<Kick> GetKick(int kickID) => await context.ModKicks.FirstOrDefaultAsync(x => x.Id == kickID);

        public async Task<int> GetKicks(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return 0;
            }

            var bans = context.ModKicks.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans.Count;
        }

        public async Task<ModerationProfile> GetModerationProfile(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            return profile;
        }

        public async Task<ModerationProfile> GetModerationProfile(int id) => await context.ModerationProfiles.FirstOrDefaultAsync(x => x.Id == id);

        public async Task<Rule> GetRule(ulong guildId, int index)
        {
            if (index < 0)
                return null;

            var profile = await context.ServerProfiles.FirstOrDefaultAsync(x => x.GuildId == (long)guildId).ConfigureAwait(false);
            var rules = context.ServerRules.AsQueryable().Where(x => x.ServerProfileId == profile.Id).ToList();

            return index >= rules.Count ? null : rules[index];
        }

        public async Task<bool> SetMuteRoleId(ulong id, ulong roleId)
        {
            var profile = await context.ServerProfiles.FirstOrDefaultAsync(x => x.GuildId == (long)id);
            if (profile == null)
                profile = await CreateServerProfile(id);

            profile.MutedRoleId = (long)roleId;
            var updateEntry = context.ServerProfiles.Update(profile);

            await context.SaveChangesAsync();
            updateEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return true;
        }

        public async Task<ulong> GetMuteRoleId(ulong id) => (ulong)(await context.ServerProfiles.FirstOrDefaultAsync(x => x.GuildId == (long)id)).MutedRoleId;

        public async Task<int> AddMute(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var mute = new Mute { Reason = reason, Time = time, ModeratorID = (long)moderatorID };
            profile.Mutes.Add(mute);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return mute.Id;
        }

        public async Task<Mute> GetMute(int muteId) => await context.ModMutes.FirstOrDefaultAsync(x => x.Id == muteId);

        public async Task<int> GetMutes(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return 0;
            }

            var mutes = context.ModMutes.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return mutes.Count;
        }
    }
}
