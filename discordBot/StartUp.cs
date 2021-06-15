using System;
using DiscordBotDB.Dal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KunalsDiscordBot
{
    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite("Data Source=Data.db", x => x.MigrationsAssembly("DiscordBotDB.Dal.Migrations"));
            });

            var serviceProvider = services.BuildServiceProvider();

            var bot = new Bot(serviceProvider);
            services.AddSingleton(bot);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {

        }
    }
}
