using System;
namespace KunalsDiscordBot.Core.Configurations
{
    public class RedditAppConfig
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
        public string RefreshToken { get; set; }
        public int PostLimit { get; set; }
    }
}
