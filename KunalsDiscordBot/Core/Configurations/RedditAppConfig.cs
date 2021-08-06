using System;
namespace KunalsDiscordBot.Core.Configurations
{
    public class RedditAppConfig
    {
        public string appId { get; set; }
        public string appSecret { get; set; }
        public string refreshToken { get; set; }
        public int postLimit { get; set; }
    }
}
