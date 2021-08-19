using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.FunCommands;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands;

namespace KunalsDiscordBot.Core.Configurations
{
    public class PepperConfigurationManager
    {
        //general
        public PepperBotConfig botConfig { get; private set; } = FromJsonFile<PepperBotConfig>("Config.json");

        //module specific
        public FunData funData { get; private set; } = FromJsonFile<FunData>("Modules", "Fun", "FunData.json");    
        public ImageData imageData { get; private set; } = FromJsonFile<ImageData>("Modules","Images","ImageData.json");
        public CurrencyData currenyConfig { get; private set; } = FromJsonFile<CurrencyData>("Modules", "Currency", "CurrencyData.json");
        public Dictionary<string, string> graphAttributes { get; private set; } = FromJsonFile<Dictionary<string, string>>("Modules", "Math", "Attributes.json");

        public ServerConfigData ServerConfigData { get; private set; } = FromJsonFile<ServerConfigData>("Modules", "General", "ConfigValues.json");

        public static T FromJsonFile<T>(params string[] path)
        {
            var jsonString = File.ReadAllText(Path.Combine(path));

            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
