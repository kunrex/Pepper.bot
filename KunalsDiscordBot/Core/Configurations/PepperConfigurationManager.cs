using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Core.ArgumentConverters;
using KunalsDiscordBot.Core.Modules.FunCommands;
using KunalsDiscordBot.Core.Modules.GameCommands;
using KunalsDiscordBot.Core.Modules.ImageCommands;
using KunalsDiscordBot.Core.Modules.MusicCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands;
using KunalsDiscordBot.Core.Modules.ImageCommands.Enums;

namespace KunalsDiscordBot.Core.Configurations
{
    public class PepperConfigurationManager
    {
        //general
        public PepperBotConfig BotConfig { get; private set; } = FromJsonFile<PepperBotConfig>("Config.json");

        //module specific
        public FunModuleData FunData { get; private set; } = FromJsonFile<FunModuleData>("Modules", "Fun", "FunData.json");    
        public ImageModuleData ImageData { get; private set; } = FromJsonFile<ImageModuleData>("Modules","Images","ImageData.json");
        public CurrencyModuleData CurrenyConfig { get; private set; } = FromJsonFile<CurrencyModuleData>("Modules", "Currency", "CurrencyData.json");
        public MusicModuleData MusicConfig { get; private set; } = FromJsonFile<MusicModuleData>("Modules", "Music", "MusicConfig.json");
        public Dictionary<string, string> GraphAttributes { get; private set; } = FromJsonFile<Dictionary<string, string>>("Modules", "Math", "Attributes.json");

        public ServerConfigData ServerConfigData { get; private set; } = FromJsonFile<ServerConfigData>("Modules", "General", "ConfigValues.json");

        //enums
        public Type[] EnumConvertorTypes { get; private set; } = new[]
        {
            typeof(Colors),
            typeof(Deforms),
            typeof(ColorScales),
            typeof(RedditPostFilter),
            typeof(RockPaperScissorsChoice),
        };

        public static T FromJsonFile<T>(params string[] path)
        {
            var jsonString = File.ReadAllText(Path.Combine(path));

            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
