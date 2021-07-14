using System;
using System.IO;
using System.Threading.Tasks;
using Reddit;
using System.Linq;
using Reddit.Controllers;
using System.Collections.Generic;
using Reddit.Controllers.EventArgs;

namespace KunalsDiscordBot.Reddit
{
    public enum RedditPostFilter
    {
        New,
        Hot,
        Top
    }

    public sealed class RedditApp
    {
        public RedditClient client { get; private set; }
        private readonly Config configuration = System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(Path.Combine("Reddit", "RedditConfig.json")));

        private List<Post> memes { get; set; } = new List<Post>();
        private List<Post> nonNSFWMemes { get; set; } = new List<Post>();

        private List<Post> animals { get; set; } = new List<Post>();
        private List<Post> awww { get; set; } = new List<Post>();

        public RedditApp()
        {
            client = new RedditClient(appId: configuration.appId, appSecret: configuration.appSecret, refreshToken: configuration.refreshToken);

            memes = SubRedditSetUp("memes", (s, e) => OnMemePostAdded(s, e));
            animals = SubRedditSetUp("Animals", (s, e) => OnAnimalPostAdded(s, e));
            awww = SubRedditSetUp("aww", (s, e) => OnAwwPostAdded(s, e));

            nonNSFWMemes = memes.Where(x => !x.NSFW).ToList();
        }

        public Subreddit GetSubReddit(string subreddit)
        {
            try
            {
                return client.Subreddit(subreddit)?.About();
            }
            catch
            {
                return null;
            }
        }

        public Post GetRandomPost(Subreddit subreddit, bool allowNSFW = false, bool onlyImages = false, RedditPostFilter filter = RedditPostFilter.New)
        {
            // Get the top post from a subreddit
            var filtered = Filter(subreddit.Posts, allowNSFW, onlyImages, filter);           

            return filtered == null ? null : filtered[new Random().Next(0, filtered.Count)];
        }

        private List<Post> Filter(SubredditPosts posts, bool allowNSFW = false, bool onlyImages = false, RedditPostFilter filter = RedditPostFilter.New)
        {
            var filtered = new List<Post>();

            switch (filter)
            {
                case RedditPostFilter.New:
                    filtered = posts.New.Take(configuration.postLimit).ToList();
                    break;
                case RedditPostFilter.Hot:
                    filtered = posts.Hot.Take(configuration.postLimit).ToList();
                    break;
                case RedditPostFilter.Top:
                    filtered = posts.Top.Take(configuration.postLimit).ToList();
                    break;
            }

            if (filtered.Count == 0 || filtered == null)
                return null;

            if (!allowNSFW)
                filtered = filtered.Where(x => !x.NSFW).ToList();
            if (filtered.Count == 0 || filtered == null)
                return null;

            if (onlyImages)
                filtered = filtered.Where(x => x.Listing.URL.EndsWith(".png") || x.Listing.URL.EndsWith(".jpeg") || x.Listing.URL.EndsWith(".gif")).ToList();
            if (filtered.Count == 0 || filtered == null)
                return null;

            return filtered;
        }

        private List<Post> SubRedditSetUp(string subredditName, EventHandler<PostsUpdateEventArgs> action)
        {
            var subReddit = client.Subreddit(subredditName).About();

            var posts = new List<Post>();

            posts.AddRange(subReddit.Posts.New.Where(x => x.Listing.URL.EndsWith(".png") || x.Listing.URL.EndsWith(".jpeg") || x.Listing.URL.EndsWith(".gif"))
                .Take(configuration.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Hot.Where(x => x.Listing.URL.EndsWith(".png") || x.Listing.URL.EndsWith(".jpeg") || x.Listing.URL.EndsWith(".gif"))
                .Take(configuration.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Top.Where(x => x.Listing.URL.EndsWith(".png") || x.Listing.URL.EndsWith(".jpeg") || x.Listing.URL.EndsWith(".gif"))
                .Take(configuration.postLimit).ToList());

            //subscribe to all events
            subReddit.Posts.NewUpdated += action;
            subReddit.Posts.MonitorNew();

            subReddit.Posts.HotUpdated += action;
            subReddit.Posts.MonitorHot();

            subReddit.Posts.TopUpdated += action;
            subReddit.Posts.MonitorTop();

            return posts;
        }

        public Post GetMeme(bool allowNSFW = false) => !allowNSFW ? nonNSFWMemes[new Random().Next(0, nonNSFWMemes.Count)] : memes[new Random().Next(0, memes.Count)];
        public Post GetAnimals() => animals[new Random().Next(0, animals.Count)];
        public Post GetAww() => awww[new Random().Next(0, awww.Count)];

        public void OnMemePostAdded(object sender, PostsUpdateEventArgs e)
        {
            if (memes == null)
                return;

            foreach (var post in e.Added)
                if (post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif"))
                {
                    memes.RemoveAt(0);//cycle
                    memes.Add(post);
                }
        }

        public void OnAnimalPostAdded(object sender, PostsUpdateEventArgs e)
        {
            if (animals == null)
                return;

            foreach (var post in e.Added)
                if (post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif"))
                {
                    animals.RemoveAt(0);//cycle
                    animals.Add(post);
                }
        }

        public void OnAwwPostAdded(object sender, PostsUpdateEventArgs e)
        {
            if (awww == null)
                return;

            foreach (var post in e.Added)
                if (post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif"))
                {
                    awww.RemoveAt(0);//cycle
                    awww.Add(post);
                }
        }

        private class Config
        {
            public string appId { get; set; }
            public string appSecret { get; set; }
            public string refreshToken { get; set; }
            public int postLimit { get; set; }
        }
    }
}
