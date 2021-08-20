using System;
namespace KunalsDiscordBot.Core.Reddit
{
    public struct RedditFilter
    {
        public bool AllowNSFW { get; set; }
        public bool ImagesOnly { get; set; }
        public RedditPostFilter? Filter { get; set; }
        public int Take { get; set; }
    }
}
