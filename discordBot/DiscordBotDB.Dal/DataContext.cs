using System;
using DiscordBotDB.Dal.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotDB.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Item> items { get; set; }
    }
}
