﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Configurations;

namespace KunalsDiscordBot
{
    public sealed class PepperBotClientManager
    {
        private static Dictionary<int, PepperBot> shards { get; set; }

        public static PepperBot GetShard(int id) => shards[id];

        public PepperBotClientManager(IServiceProvider services)
        {
            var botConfiguration = (PepperConfigurationManager)services.GetService(typeof(PepperConfigurationManager));

            int shardCount = botConfiguration.BotConfig.DiscordConfig.ShardCount;
            if (shardCount <= 0)
                throw new Exception("Invalid Shard Count");

            shards = new Dictionary<int, PepperBot>();
            for (int i = 0; i < shardCount; i++)
                shards.Add(i, new PepperBot(services, botConfiguration, i));

            Task.Run(async() =>
            {
                foreach (var shard in shards)
                    await shard.Value.ConnectAsync();

                if (botConfiguration.BotConfig.DiscordConfig.WriteCommands)
                    shards[0].Commands.WriteAllCommands(botConfiguration.BotConfig.DiscordConfig.WriteLocation);
            });
        }
    }
}
