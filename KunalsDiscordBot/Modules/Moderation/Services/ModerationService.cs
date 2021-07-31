using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Moderation;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

using System.Linq;
using System.Collections.Generic;

using KunalsDiscordBot.Events;
using DSharpPlus.Entities;
using DiscordBotDataBase.Dal.Models.Servers;

namespace KunalsDiscordBot.Services.Moderation
{
    public class ModerationService : IModerationService
    {
        private readonly DataContext context;

        public ModerationService(DataContext _context) => context = _context;

        public async Task<int> AddBan(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

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
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

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
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

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
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var kick = new Kick { Reason = reason, ModeratorID = (long)moderatorID };
            profile.Kicks.Add(kick);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return kick.Id;
        }

        public async Task<bool> ClearEndorsements(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);
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
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);
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

        public Task<Ban> GetBan(int banID) => Task.FromResult(context.ModBans.FirstOrDefault(x => x.Id == banID));

        public async Task<List<Ban>> GetBans(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return profile.Bans;
            }

            var bans = context.ModBans.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans;
        }

        public Task<Endorsement> GetEndorsement(int endorsementID) => Task.FromResult(context.ModEndorsements.FirstOrDefault(x => x.Id == endorsementID));

        public async Task<List<Endorsement>> GetEndorsements(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return profile.Endorsements;
            }

            var endorsements = context.ModEndorsements.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return endorsements;
        }

        public Task<Infraction> GetInfraction(int infractionID) => Task.FromResult(context.ModInfractions.FirstOrDefault(x => x.Id == infractionID));

        public async Task<List<Infraction>> GetInfractions(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return profile.Infractions;
            }

            var infractions = context.ModInfractions.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return infractions;
        }

        public Task<Kick> GetKick(int kickID) => Task.FromResult(context.ModKicks.FirstOrDefault(x => x.Id == kickID));

        public async Task<List<Kick>> GetKicks(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return profile.Kicks;
            }

            var kicks = context.ModKicks.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return kicks;
        }

        public async Task<ModerationProfile> GetModerationProfile(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            return profile;
        }

        public Task<ModerationProfile> GetModerationProfile(int id) => Task.FromResult(context.ModerationProfiles.FirstOrDefault(x => x.Id == id));

        public async Task<int> AddMute(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateModerationProfile(id, guildId);

            var mute = new Mute { Reason = reason, Time = time, StartTime = DateTime.Now.ToString("dddd, dd MMMM yyyy"), ModeratorID = (long)moderatorID };
            profile.Mutes.Add(mute);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return mute.Id;
        }

        public Task<Mute> GetMute(int muteId) => Task.FromResult(context.ModMutes.FirstOrDefault(x => x.Id == muteId));

        public async Task<List<Mute>> GetMutes(ulong id, ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateModerationProfile(id, guildId);
                return profile.Mutes;
            }

            var mutes = context.ModMutes.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return mutes;
        }

        public Task<List<Mute>> GetMutes(ulong guildId)
        {
            var profile = context.ModerationProfiles.FirstOrDefault(x => x.GuildId == (long)guildId);

            if (profile == null)
                return null;

            var mutes = context.ModMutes.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return Task.FromResult(mutes);
        }

        public async Task<bool> ClearAllServerModerationData(ulong serverId)
        {
            foreach (var ban in context.ModBans.Where(x => x.GuildID == (long)serverId).ToList())
            {
                var entry = context.Remove(ban);
                await context.SaveChangesAsync();

                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }


            foreach (var infraction in context.ModInfractions.Where(x => x.GuildID == (long)serverId).ToList())
            {
                var entry = context.Remove(infraction);
                await context.SaveChangesAsync();

                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            foreach (var mute in context.ModMutes.Where(x => x.GuildID == (long)serverId).ToList())
            {
                var entry = context.Remove(mute);
                await context.SaveChangesAsync();

                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            foreach (var endorse in context.ModEndorsements.Where(x => x.GuildID == (long)serverId).ToList())
            {
                var entry = context.Remove(endorse);
                await context.SaveChangesAsync();

                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            foreach (var kick in context.ModKicks.Where(x => x.GuildID == (long)serverId).ToList())
            {
                var entry = context.Remove(kick);
                await context.SaveChangesAsync();

                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }

            return true;
        }
    }
}
