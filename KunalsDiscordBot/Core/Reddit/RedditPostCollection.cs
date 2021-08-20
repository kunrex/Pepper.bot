using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KunalsDiscordBot.Core.Configurations;
using Reddit;
using Reddit.Controllers;
using Reddit.Controllers.EventArgs;
using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Core.Reddit
{
    public class RedditPostCollection
    {
        public string subRedditName { get; private set; }
        private List<Post> posts { get; set; } = new List<Post>();

        public int count
        {
            get => posts == null ? 0 : posts.Count;
        }

        public RedditPostCollection(string subReddit) => subRedditName = subReddit;

        public async Task<RedditPostCollection> Collect(RedditClient client, RedditFilter filter)
        {
            await SetUp(client, OnPostAdded, filter);

            return this;
        }

        public Post this[int index, bool allowNSFW = true]
        {
            get => allowNSFW ? posts[index] : posts.Where(x => !x.NSFW).ToList()[index];
        }

        private Task SetUp(RedditClient client, EventHandler<PostsUpdateEventArgs> action, RedditFilter filter)
        {
            var subReddit = client.Subreddit(subRedditName).About();
            posts = subReddit.Posts.FilterPosts(filter);

            //subscribe to all events
            subReddit.Posts.NewUpdated += action;
            subReddit.Posts.MonitorNew();

            subReddit.Posts.HotUpdated += action;
            subReddit.Posts.MonitorHot();

            subReddit.Posts.TopUpdated += action;
            subReddit.Posts.MonitorTop();

            return Task.CompletedTask;
        }

        public void OnPostAdded(object sender, PostsUpdateEventArgs e) => Task.Run(() =>
        {
            foreach (var post in e.Added)
                if (post.IsValidDiscordPost())
                {
                    posts.RemoveAt(0);//cycle
                    posts.Add(post);
                }
        });
    }
}
