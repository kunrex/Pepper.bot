using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Reddit.Exceptions;

namespace KunalsDiscordBot.Core.Reddit
{
    public class RedditPostCollection
    {
        public string SubRedditName { get; private set; }
        private List<Post> Posts { get; set; } = new List<Post>();

        public int Count
        {
            get => Posts == null ? 0 : Posts.Count;
        }

        public RedditPostCollection(string subReddit) => SubRedditName = subReddit;

        public async Task<RedditPostCollection> Collect(RedditClient client, RedditFilter filter)
        {
            await SetUp(client, OnPostAdded, filter);

            return this;
        }

        public Post GetRandomPost(bool nsfw)
        {
            if (nsfw)
                return this[new Random().Next(0, Posts.Count)];
            else
            {
                var filtered = Posts.Where(x => !x.NSFW).ToList();

                return filtered[new Random().Next(0, filtered.Count)];
            }
        }

        public Post this[int index] => Posts[index];

        private Task SetUp(RedditClient client, EventHandler<PostsUpdateEventArgs> action, RedditFilter filter)
        {
            var subReddit = client.Subreddit(SubRedditName).About();
            if (subReddit == null)
                throw new RedditPostCollectionException(SubRedditName, "Invalid subreddit given");

            Posts = subReddit.Posts.FilterPosts(filter);

            //subscribe to all events
            switch(filter.Filter)
            {
                case RedditPostFilter.New:
                    subReddit.Posts.NewUpdated += action;
                    subReddit.Posts.MonitorNew();
                    break;
                case RedditPostFilter.Hot:
                    subReddit.Posts.HotUpdated += action;
                    subReddit.Posts.MonitorHot();
                    break;
                case RedditPostFilter.Top:
                    subReddit.Posts.TopUpdated += action;
                    subReddit.Posts.MonitorTop();
                    break;
                default:
                    subReddit.Posts.NewUpdated += action;
                    subReddit.Posts.MonitorNew();
                    subReddit.Posts.HotUpdated += action;
                    subReddit.Posts.MonitorHot();
                    goto case RedditPostFilter.Top;
            }
            
            return Task.CompletedTask;
        }

        public void OnPostAdded(object sender, PostsUpdateEventArgs e) => Task.Run(() =>
        {
            foreach (var post in e.Added)
                if (post.IsValidDiscordPost())
                {
                    Posts.RemoveAt(0);//cycle
                    Posts.Add(post);
                }
        });
    }
}
