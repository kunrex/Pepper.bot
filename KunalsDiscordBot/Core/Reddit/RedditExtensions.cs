using System;
using System.Collections.Generic;
using System.Linq;
using Reddit;
using Reddit.Controllers;

namespace KunalsDiscordBot.Core.Reddit
{
    public static class RedditExtensions
    {
        public static bool IsValidDiscordPost(this Post post) => post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif");

        public static List<Post> FilterPosts(this SubredditPosts posts, RedditFilter filter)
        {
            var filtered = new List<Post>();

            switch (filter.filter)
            {
                case RedditPostFilter.New:
                    filtered = posts.New.Take(filter.take).ToList();
                    break;
                case RedditPostFilter.Hot:
                    filtered = posts.Hot.Take(filter.take).ToList();
                    break;
                case RedditPostFilter.Top:
                    filtered = posts.Top.Take(filter.take).ToList();
                    break;
            }

            if (filtered.Count == 0 || filtered == null)
                return null;

            if (!filter.allowNSFW)
                filtered = filtered.Where(x => !x.NSFW).ToList();
            if (filtered.Count == 0 || filtered == null)
                return null;

            if (filter.imagesOnly)
                filtered = filtered.Where(x => x.IsValidDiscordPost()).ToList();

            if (filtered.Count == 0 || filtered == null)
                return null;

            return filtered;
        }
    }
}
