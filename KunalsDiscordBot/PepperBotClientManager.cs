using System;
using System.Collections.Generic;
using KunalsDiscordBot.Core.Configurations;

namespace KunalsDiscordBot
{
    public sealed class PepperBotClientManager
    {
        private static Dictionary<int, PepperBot> shards { get; set; }

        public PepperBotClientManager(IServiceProvider services)
        {
            var botConfiguration = ((PepperConfigurationManager)services.GetService(typeof(PepperConfigurationManager))).botConfig;

            int shardCount = botConfiguration.discordConfig.shardCount;
            if (shardCount <= 0)
                throw new Exception("Invalid Shard Count");

            shards = new Dictionary<int, PepperBot>();
            for (int i = 0; i < shardCount; i++)
                shards.Add(i, new PepperBot(services, botConfiguration, i));

            BootShards();
        }

        private async void BootShards()
        {
            foreach (var shard in shards)
                await shard.Value.ConnectAsync();
        }
    }
}
