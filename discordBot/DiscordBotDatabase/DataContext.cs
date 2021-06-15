using System;
using DiscordBotDatabase.Models.Items;
using Microsoft.EntityFrameworkCore;

namespace DiscordBotDatabase
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        public DbSet<Item> Items { get; set; }
    }
}
