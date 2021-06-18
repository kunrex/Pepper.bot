using System;
using DiscordBotDataBase.Dal.Models.Items;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DiscordBotDataBase.Dal.Models.Profile;

namespace DiscordBotDataBase.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Profile> UserProfiles { get; set; }
        public DbSet<ItemDBData> ProfileItems { get; set; }
    }
}
