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
                profile = await CreateProfile(id, guildId);

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
                profile = await CreateProfile(id, guildId);

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
                profile = await CreateProfile(id, guildId);

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
                profile = await CreateProfile(id, guildId);

            var kick = new Kick { Reason = reason, ModeratorID = (long)moderatorID };
            profile.Kicks.Add(kick);

            var update = context.ModerationProfiles.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return kick.Id;
        }

        public async Task<ModerationProfile> CreateProfile(ulong id, ulong guildId)
        {
            var modProfile = new ModerationProfile { DiscordId = (long)id, GuildId = (long)guildId };

            await context.AddAsync(modProfile);
            await context.SaveChangesAsync();

            return modProfile;
        }

        public async Task<Ban> GetBan(int banID) => await context.ModBans.FirstOrDefaultAsync(x => x.Id == banID);

        public async Task<int> GetBans(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
            {
                profile = await CreateProfile(id, guildId);
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
                profile = await CreateProfile(id, guildId);
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
                profile = await CreateProfile(id, guildId);
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
                profile = await CreateProfile(id, guildId);
                return 0;
            }

            var bans = context.ModKicks.AsQueryable().Where(x => x.ModerationProfileId == profile.Id).ToList();
            return bans.Count;
        }

        public async Task<ModerationProfile> GetProfile(ulong id, ulong guildId)
        {
            var profile = await context.ModerationProfiles.FirstOrDefaultAsync(x => x.DiscordId == (long)id && x.GuildId == (long)guildId);

            if (profile == null)
                profile = await CreateProfile(id, guildId);

            return profile;
        }

        public async Task<ModerationProfile> GetProfile(int id) => await context.ModerationProfiles.FirstOrDefaultAsync(x => x.Id == id);
    }
}
