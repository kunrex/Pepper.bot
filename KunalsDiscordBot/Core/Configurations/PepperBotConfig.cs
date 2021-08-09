using System;

namespace KunalsDiscordBot.Core.Configurations
{
    public class PepperBotConfig
    {
        public int version { get; set; }

        public DiscordConfig discordConfig { get; set; }
        public LavalinkConfig lavalinkConfig { get; set; }
        public RedditAppConfig redditConfig { get; set; }
    }
}
