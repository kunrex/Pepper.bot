﻿using System;
using DiscordBotDataBase.Dal.Models.Items;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using DiscordBotDataBase.Dal.Models.Profile;
using DiscordBotDataBase.Dal.Models.Profile.Boosts;
using DiscordBotDataBase.Dal.Models.Servers;
using DiscordBotDataBase.Dal.Models.Servers.Models;
using DiscordBotDataBase.Dal.Models.Servers.Models.Moderation;

namespace DiscordBotDataBase.Dal
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        //Currency Commands
        public DbSet<Profile> UserProfiles { get; set; }
        public DbSet<ItemDBData> ProfileItems { get; set; }
        public DbSet<BoostData> ProfileBoosts { get; set; }

        //Server Configuration and Moderation
        public DbSet<ServerProfile> ServerProfiles { get; set; }

        public DbSet<ModerationData> ModerationDatas { get; set; }
        public DbSet<FunData> FunDatas { get; set; }
        public DbSet<GameData> GameDatas { get; set; }
        public DbSet<MusicData> MusicDatas { get; set; }

        public DbSet<Rule> ServerRules { get; set; }

        public DbSet<Infraction> ModInfractions { get; set; }
        public DbSet<Endorsement> ModEndorsements { get; set; }
        public DbSet<Ban> ModBans { get; set; }
        public DbSet<Kick> ModKicks { get; set; }
        public DbSet<Mute> ModMutes { get; set; }
    }
}
