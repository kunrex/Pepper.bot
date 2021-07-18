using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DiscordBotDataBase.Dal;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Reddit;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Music;

namespace KunalsDiscordBot
{
    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("InConfigureServices");
            services.AddDbContext<DataContext>(options =>
            {
                //options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DataContext;Trusted_Connection=True;MultipleActiveResultSets=true", X => X.MigrationsAssembly("DiscordBotDataBase.Dal.Migrations"));
                options.UseSqlite("Data Source=Data.db", x => x.MigrationsAssembly("DiscordBotDataBase.Dal.Migrations"));

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddSingleton(new RedditApp());

            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IModerationService, ModerationService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IServerService, ServerService>();
            services.AddScoped<IMusicService, MusicService>();

            BuildService(services);
        }

        private void BuildService(IServiceCollection services)
        {
            Console.WriteLine("InBuildService");
            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            Console.WriteLine("InConfigure");
        }
    }
}
