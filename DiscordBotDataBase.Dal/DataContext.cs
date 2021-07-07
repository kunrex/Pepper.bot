using System;
using DiscordBotDataBase.Dal.Models.Items;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DiscordBotDataBase.Dal.Models.Profile;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;
using DiscordBotDataBase.Dal.Models.Moderation;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

namespace DiscordBotDataBase.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Profile> UserProfiles { get; set; }
        public DbSet<ItemDBData> ProfileItems { get; set; }
        public DbSet<BoostData> ProfileBoosts { get; set; }

        public DbSet<ModerationProfile> ModerationProfiles { get; set; }

        public DbSet<Infraction> ModInfractions { get; set; }
        public DbSet<Endorsement> ModEndorsements { get; set; }
        public DbSet<Ban> ModBans { get; set; }
        public DbSet<Kick> ModKicks { get; set; }
        public DbSet<Mute> ModMutes { get; set; }

        public DbSet<ServerProfile> ServerProfiles { get; set; }
        public DbSet<Rule> ServerRules { get; set; }
    }
}
