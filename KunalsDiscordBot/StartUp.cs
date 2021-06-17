using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DiscordBotDataBase.Dal;

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
