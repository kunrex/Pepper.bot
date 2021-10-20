using System;

namespace KunalsDiscordBot.Core.Reddit.Exceptions
{
    public class RedditPostCollectionException : Exception
    {
        public RedditPostCollectionException(string subreddit, string message) : base($"Exception occured in collecting posts for {subreddit}. Message: {message}")
        {

        }
    }
}
