using System;
using System.Text;
using System.Text.Json;
using System.IO;

namespace KunalsDiscordBot.Core.Configurations
{
    public class PepperConfigurationManager
    {
        public RedditAppConfig redditAppConfig { get; set; } = FromJsonFile<RedditAppConfig>("Core", "Reddit", "RedditConfig.json");

        public static T FromJsonFile<T>(params string[] path)
        {
            var jsonString = File.ReadAllText(Path.Combine(path));

            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
