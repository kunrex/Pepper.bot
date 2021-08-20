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

            switch (filter.Filter)
            {
                case RedditPostFilter.New:
                    filtered = posts.New.Take(filter.Take).ToList();
                    break;
                case RedditPostFilter.Hot:
                    filtered = posts.Hot.Take(filter.Take).ToList();
                    break;
                case RedditPostFilter.Top:
                    filtered = posts.Top.Take(filter.Take).ToList();
                    break;
                default:
                    filtered.AddRange(posts.New.Take(filter.Take).ToList());
                    filtered.AddRange(posts.Hot.Take(filter.Take).ToList());
                    filtered.AddRange(posts.Top.Take(filter.Take).ToList());
                    break;
            }

            if (filtered.IsNullOrEmpty())
                return null;

            if (!filter.AllowNSFW)
            {
                filtered = filtered.Where(x => !x.NSFW).ToList();

                if (filtered.IsNullOrEmpty())
                    return null;
            }

            if (filter.ImagesOnly)
                filtered = filtered.Where(x => x.IsValidDiscordPost()).ToList();

            return filtered;
        }

        public static bool IsNullOrEmpty<T>(this IList<T> list) => list == null || list.Count == 0;
    }
}
