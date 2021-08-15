using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal;

using System.Linq;
using System.Collections.Generic;

using KunalsDiscordBot.Events;
using DSharpPlus.Entities;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;

namespace KunalsDiscordBot.Services.Moderation
{
    public class ModerationService : IModerationService
    {
        private readonly DataContext context;

        public ModerationService(DataContext _context) => context = _context;

        public async Task<int> AddBan(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId);

            var ban = new Ban { Reason = reason, Time = time , ModeratorID = (long)moderatorID, UserId = (long)id };
            profile.Bans.Add(ban);

            var update = context.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return ban.Id;
        }

        public async Task<int> AddEndorsement(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId);

            var endorsement = new Endorsement { Reason = reason, ModeratorID = (long)moderatorID, UserId = (long)id };
            profile.Endorsements.Add(endorsement);

            var update = context.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return endorsement.Id;
        }

        public async Task<int> AddInfraction(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId);

            var infraction = new Infraction { Reason = reason, ModeratorID = (long)moderatorID, UserId = (long)id };
            profile.Infractions.Add(infraction);

            var update = context.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return infraction.Id;
        }

        public async Task<int> AddKick(ulong id, ulong guildId, ulong moderatorID, string reason)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId);

            var kick = new Kick { Reason = reason, ModeratorID = (long)moderatorID, UserId = (long)id };
            profile.Kicks.Add(kick);

            var update = context.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return kick.Id;
        }

        public async Task<int> AddMute(ulong id, ulong guildId, ulong moderatorID, string reason, string time)
        {
            var profile = context.ModerationDatas.FirstOrDefault(x => x.Id == (long)guildId);

            var mute = new Mute { Reason = reason, Time = time, StartTime = DateTime.Now.ToString("dddd, dd MMMM yyyy"), ModeratorID = (long)moderatorID, UserId = (long)id };
            profile.Mutes.Add(mute);

            var update = context.Update(profile);
            await context.SaveChangesAsync();

            update.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            return mute.Id;
        }

        public async Task<bool> ClearEndorsements(ulong id, ulong guildId)
        {
            var endorsements = context.ModEndorsements.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList();
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
            var infractions = context.ModInfractions.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList();
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

        public Task<Ban> GetBan(int banID) => Task.FromResult(context.ModBans.FirstOrDefault(x => x.Id == banID));

        public Task<List<Ban>> GetBans(ulong id, ulong guildId) => Task.FromResult(context.ModBans.AsQueryable().Where(x => x.ModerationDataId == (long)id).Where(x => x.UserId == (long)guildId).ToList());

        public Task<Endorsement> GetEndorsement(int endorsementID) => Task.FromResult(context.ModEndorsements.FirstOrDefault(x => x.Id == endorsementID));

        public Task<List<Endorsement>> GetEndorsements(ulong id, ulong guildId) => Task.FromResult(context.ModEndorsements.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList());

        public Task<Infraction> GetInfraction(int infractionID) => Task.FromResult(context.ModInfractions.FirstOrDefault(x => x.Id == infractionID));

        public Task<List<Infraction>> GetInfractions(ulong id, ulong guildId) => Task.FromResult(context.ModInfractions.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList());

        public Task<Kick> GetKick(int kickID) => Task.FromResult(context.ModKicks.FirstOrDefault(x => x.Id == kickID));

        public Task<List<Kick>> GetKicks(ulong id, ulong guildId) => Task.FromResult(context.ModKicks.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList());

        public Task<Mute> GetMute(int muteId) => Task.FromResult(context.ModMutes.FirstOrDefault(x => x.Id == muteId));

        public Task<List<Mute>> GetMutes(ulong id, ulong guildId) => Task.FromResult(context.ModMutes.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).Where(x => x.UserId == (long)id).ToList());

        public Task<List<Mute>> GetMutes(ulong guildId) => Task.FromResult(context.ModMutes.AsQueryable().Where(x => x.ModerationDataId == (long)guildId).ToList());

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
