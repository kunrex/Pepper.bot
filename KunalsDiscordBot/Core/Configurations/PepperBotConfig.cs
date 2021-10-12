using System;

namespace KunalsDiscordBot.Core.Configurations
{
    public class PepperBotConfig
    {
        public int Version { get; set; }

        public DiscordConfig DiscordConfig { get; set; }
        public LavalinkConfig LavalinkConfig { get; set; }
        public RedditAppConfig RedditConfig { get; set; }
        public ChatbotConfig ChatbotConfig { get; set; }
    }
}
