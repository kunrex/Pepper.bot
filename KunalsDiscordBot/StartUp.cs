using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DiscordBotDataBase.Dal;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Services.Moderation;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Music;
using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Services.Configuration;

namespace KunalsDiscordBot
{
    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("InConfigureServices");
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite("Data Source=Data.db", x => x.MigrationsAssembly("DiscordBotDataBase.Dal.Migrations"));

                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            services.AddSingleton<PepperConfigurationManager>();

            services.AddSingleton<RedditApp>()
                .AddScoped<IProfileService, ProfileService>()
                .AddScoped<IModerationService, ModerationService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IServerService, ServerService>()
                .AddScoped<IMusicService, MusicService>()
                .AddScoped<IConfigurationService, ConfigurationService>();

            BuildService(services);
        }

        private void BuildService(IServiceCollection services)
        {
            Console.WriteLine("InBuildService");
            var serviceProvider = services.BuildServiceProvider();

            var botManager = new PepperBotClientManager(serviceProvider);
            services.AddSingleton(botManager);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment) => Console.WriteLine("InConfigure");
    }
}
