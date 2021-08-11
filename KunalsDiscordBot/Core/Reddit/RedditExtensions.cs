using System;
using System.Collections.Generic;
using System.Linq;
using KunalsDiscordBot.Core.Reddit;
using Reddit;
using Reddit.Controllers;

namespace KunalsDiscordBot.Extensions
{
    public static class RedditExtensions
    {
        public static bool IsValidDiscordPost(this Post post) => post.Listing.URL.Contains(".jpg") || post.Listing.URL.Contains(".png")
            || (post.Listing.URL.Contains(".gif") && !post.Listing.URL.Contains(".gifv"));

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

            if (filtered.IsNullOrEmpty())
                return null;

            if (!filter.allowNSFW)
                filtered = filtered.Where(x => !x.NSFW).ToList();
            if (filtered.IsNullOrEmpty())
                return null;

            if (filter.imagesOnly)
                filtered = filtered.Where(x => x.IsValidDiscordPost()).ToList();

            return filtered;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list.Count == 0;
    }
}
