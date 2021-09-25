using System;
namespace KunalsDiscordBot.Core.Configurations
{
    public class DiscordConfig
    {
        public string Token { get; set; }
        public string[] Prefixes { get; set; }

        public bool Dms { get; set; }

        public int TimeOut { get; set; }
        public int ShardCount { get; set; }

        public int ActivityType { get; set; }
        public string ActivityText { get; set; }

        public string ErrorLink { get; set; }
    }
}
