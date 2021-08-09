using System;
namespace KunalsDiscordBot.Core.Configurations
{
    public class DiscordConfig
    {
        public string token { get; set; }
        public string[] prefixes { get; set; }

        public bool dms { get; set; }

        public int timeOut { get; set; }
        public int shardCount { get; set; }

        public int activityType { get; set; }
        public string activityText { get; set; }

        public string errorLink { get; set; }
    }
}
