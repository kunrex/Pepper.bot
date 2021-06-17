using System;
using DiscordBotDataBase.Dal.Models.Items;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DiscordBotDataBase.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Item> Items { get; set; }
    }
}
