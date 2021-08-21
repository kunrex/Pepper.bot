using System;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static string GetYoutubeThumbailURL(this string url)
        {
            string imageURL = string.Empty, watchString = string.Empty;
            bool watchStringFound = false;
            int numOfSlashes = 0;

            foreach (var character in url)
            {
                if (character.Equals('/'))
                    numOfSlashes++;

                if (numOfSlashes == 3)
                {
                    if (watchStringFound)
                        imageURL += character;
                    else
                    {
                        watchString += character;

                        if (watchString == "/watch?v=")
                            watchStringFound = true;
                    }
                }
            }

            return imageURL;
        }
    }
}
