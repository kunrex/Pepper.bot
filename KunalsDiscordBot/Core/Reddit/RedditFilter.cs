using System;
namespace KunalsDiscordBot.Core.Reddit
{
    public struct RedditFilter
    {
        public bool allowNSFW { get; set; }
        public bool imagesOnly { get; set; }
        public RedditPostFilter filter;
        public int take { get; set; }
    }
}
