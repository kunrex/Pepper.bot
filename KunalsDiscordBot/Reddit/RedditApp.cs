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
    public sealed class RedditApp
    {
        public RedditClient client { get; private set; }
        private readonly Config configuration = System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(Path.Combine("Reddit", "RedditConfig.json")));

        private List<Post> memes { get; set; } = new List<Post>();
        private List<Post> animals { get; set; } = new List<Post>();
        private List<Post> awww { get; set; } = new List<Post>();

        public RedditApp()
        {
            client = new RedditClient(appId: configuration.appId, appSecret: configuration.appSecret, refreshToken: configuration.refreshToken);

            memes = SubRedditSetUp("memes", (s, e) => OnMemePostAdded(s, e));
            animals = SubRedditSetUp("Animals", (s, e) => OnAnimalPostAdded(s, e));
            awww = SubRedditSetUp("aww", (s, e) => OnAwwPostAdded(s, e));
        }

        public Post GetRandomPost(string subreddit, bool useImages = true)
        {
            var askReddit = client.Subreddit(subreddit).About();
            // Get the top post from a subreddit
            var filtered = askReddit.Posts.New.Where(x => x.Listing.IsSelf != useImages && !x.Listing.IsVideo).ToList();

            return askReddit.Posts.New[new Random().Next(0, askReddit.Posts.New.Count)];
        }

        private List<Post> SubRedditSetUp(string subredditName, EventHandler<PostsUpdateEventArgs> action)
        {
            var subReddit = client.Subreddit(subredditName).About();

            var posts = new List<Post>();

            posts.AddRange(subReddit.Posts.New.Where(x => !x.Listing.IsSelf && !x.Listing.IsVideo).Take(configuration.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Hot.Where(x => !x.Listing.IsSelf && !x.Listing.IsVideo).Take(configuration.postLimit).ToList());
            posts.AddRange(subReddit.Posts.Top.Where(x => !x.Listing.IsSelf && !x.Listing.IsVideo).Take(configuration.postLimit).ToList());

            //subscribe to all events
            subReddit.Posts.NewUpdated += action;
            subReddit.Posts.MonitorNew();

            subReddit.Posts.HotUpdated += action;
            subReddit.Posts.MonitorHot();

            subReddit.Posts.TopUpdated += action;
            subReddit.Posts.MonitorTop();

            return posts;
        }

        public Post GetMeme() => memes[new Random().Next(0, memes.Count)];
        public Post GetAnimals() => animals[new Random().Next(0, animals.Count)];
        public Post GetAww() => awww[new Random().Next(0, awww.Count)];

        public void OnMemePostAdded(object sender, PostsUpdateEventArgs e)
        {
            foreach (var post in e.Added)
                if (!post.Listing.IsSelf && !post.Listing.IsVideo)
                {
                    memes.RemoveAt(0);//cycle
                    memes.Add(post);
                }
        }

        public void OnAnimalPostAdded(object sender, PostsUpdateEventArgs e)
        {
            foreach (var post in e.Added)
                if (!post.Listing.IsSelf && !post.Listing.IsVideo)
                {
                    animals.RemoveAt(0);//cycle
                    animals.Add(post);
                }
        }

        public void OnAwwPostAdded(object sender, PostsUpdateEventArgs e)
        {
            foreach (var post in e.Added)
                if (!post.Listing.IsSelf && !post.Listing.IsVideo)
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
