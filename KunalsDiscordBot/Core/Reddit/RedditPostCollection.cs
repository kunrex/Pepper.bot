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

        public async Task<RedditPostCollection> Collect(RedditClient client, RedditAppConfig config)
        {
            await SetUp(client, config, OnPostAdded);

            return this;
        }

        public Post this[int index]
        {
            get => posts[index];
        }

        private Task SetUp(RedditClient client, RedditAppConfig config, EventHandler<PostsUpdateEventArgs> action)
        {
            var subReddit = client.Subreddit(subRedditName).About();
            posts = new List<Post>();

            posts.AddRange(subReddit.Posts.New.Where(x => x.IsValidDiscordPost())
                .Take(config.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Hot.Where(x => x.IsValidDiscordPost())
                .Take(config.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Top.Where(x => x.IsValidDiscordPost())
                .Take(config.postLimit).ToList());

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
