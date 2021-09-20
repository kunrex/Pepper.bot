using System;
using System.Linq;
using System.Collections.Generic;

using Reddit.Controllers;

using KunalsDiscordBot.Core.Reddit;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
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
                    var toTake = filter.Take / 3;

                    filtered.AddRange(posts.New.Take(toTake).ToList());
                    filtered.AddRange(posts.Hot.Take(toTake).ToList());
                    filtered.AddRange(posts.Top.Take(toTake).ToList());
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
