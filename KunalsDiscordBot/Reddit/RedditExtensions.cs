using System;
using Reddit;
using Reddit.Controllers;

namespace KunalsDiscordBot.Reddit
{
    public static class RedditExtensions
    {
        public static bool IsValidDiscordPost(this Post post) => post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif");
    }
}
