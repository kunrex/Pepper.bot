using System;
using Reddit;
using Reddit.Controllers;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Extensions;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Reddit
{
    public enum RedditPostFilter
    {
        New,
        Hot,
        Top
    }

    public sealed class RedditApp
    {
        public RedditClient Client { get; private set; }
        public bool Online { get; private set; }

        private readonly RedditAppConfig configuration;

        private RedditPostCollection Memes { get; set; } 
        private RedditPostCollection Aww { get; set; }
        private RedditPostCollection Showerthoughts { get; set; }

        public RedditApp(PepperConfigurationManager configManager)
        {
            configuration = configManager.BotConfig.RedditConfig;
            Client = new RedditClient(appId: configuration.AppId, appSecret: configuration.AppSecret, refreshToken: configuration.RefreshToken);

            Task.Run(() => SetUpCollections());
        }

        private async Task SetUpCollections()
        {
            var filter = new RedditFilter
            {
                AllowNSFW = true,
                ImagesOnly = true,
                Take = configuration.PostLimit,
                Filter = RedditPostFilter.New
            };

            Memes = await new RedditPostCollection("memes").Collect(Client, filter);
            Aww = await new RedditPostCollection("aww").Collect(Client, filter);

            filter.ImagesOnly = false;
            Showerthoughts = await new RedditPostCollection("Showerthoughts").Collect(Client, filter);

            Online = true;
            Console.WriteLine("Reddit app online");
        }

        public Subreddit GetSubReddit(string subreddit)
        {
            try
            {
                return Client.Subreddit(subreddit)?.About();
            }
            catch
            {
                return null;
            }
        }

        public Post GetRandomPost(Subreddit subreddit, RedditFilter filter)
        {
            filter.Take = configuration.PostLimit;
            var filtered = subreddit.Posts.FilterPosts(filter);

            return filtered == null || filtered.Count == 0  ? null : filtered[new Random().Next(0, filtered.Count)];
        }

        public Post GetMeme(bool allowNSFW) => Memes.GetRandomPost(allowNSFW);
        public Post GetAww() => Aww[new Random().Next(0, Aww.count)];
        public Post GetShowerthought() => Showerthoughts[new Random().Next(0, Showerthoughts.count)];
    }
}
