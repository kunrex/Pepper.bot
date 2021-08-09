using System;
using System.IO;
using System.Threading.Tasks;
using Reddit;
using System.Linq;
using Reddit.Controllers;
using System.Collections.Generic;
using Reddit.Controllers.EventArgs;
using KunalsDiscordBot.Core.Configurations;

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
        public RedditClient client { get; private set; }
        private readonly RedditAppConfig configuration;

        private RedditPostCollection memes { get; set; } 
        private RedditPostCollection aww { get; set; }
        private RedditPostCollection animals { get; set; }

        public RedditApp(PepperConfigurationManager configManager)
        {
            configuration = configManager.botConfig.redditConfig;
            client = new RedditClient(appId: configuration.appId, appSecret: configuration.appSecret, refreshToken: configuration.refreshToken);

            SetUpCollections();
        }

        private async void SetUpCollections()
        {
            memes = await new RedditPostCollection("memes").Start(client, configuration);
            aww = await new RedditPostCollection("aww").Start(client, configuration);
            animals = await new RedditPostCollection("Animals").Start(client, configuration);

            Console.WriteLine("Reddit app online");
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

        public Post GetRandomPost(Subreddit subreddit, RedditFilter filter)
        {
            filter.take = configuration.postLimit;
            var filtered = subreddit.Posts.FilterPosts(filter);

            return filtered == null ? null : filtered[new Random().Next(0, filtered.Count)];
        }

        public Post GetMeme(bool allowNSFW = false) => allowNSFW ? memes[new Random().Next(0, memes.count)]  : memes[new Random().Next(0, memes.count)];
        public Post GetAww() => aww[new Random().Next(0, aww.count)];
        public Post GetAnimals() => animals[new Random().Next(0, aww.count)];
    }
}
