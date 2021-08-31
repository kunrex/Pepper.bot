using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using DiscordBotDataBase.Dal;

using KunalsDiscordBot.Core.Help;
using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Services.Fun;
using KunalsDiscordBot.Services.Music;
using KunalsDiscordBot.Services.Games;
using KunalsDiscordBot.Services.Images;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Services.Moderation;
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

            var configManager = new PepperConfigurationManager();

            services.AddSingleton(configManager)
                .AddSingleton(new RedditApp(configManager))
                .AddSingleton<IModuleService, ModuleService>()
                .AddScoped<IProfileService, ProfileService>()
                .AddScoped<IModerationService, ModerationService>()
                .AddScoped<IImageService, ImageService>()
                .AddScoped<IServerService, ServerService>()
                .AddScoped<IConfigurationService, ConfigurationService>()
                .AddSingleton<IMusicService, MusicService>()
                .AddSingleton<IGameService, GameService>()
                .AddSingleton<IFunService, FunService>();

            BuildService(services);
        }

        private void BuildService(IServiceCollection services)
        {
            Console.WriteLine("InBuildService");
            var serviceProvider = services.BuildServiceProvider();

            var botManager = new PepperBotClientManager(serviceProvider);
            services.AddSingleton(botManager).BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment) => Console.WriteLine("InConfigure");
    }
}
