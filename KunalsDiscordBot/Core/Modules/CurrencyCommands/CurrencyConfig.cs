using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands
{
    public class CurrencyModuleData
    {
        public int thumbnailSize { get; set; }
        public string coinsEmoji { get; set; }
        public string tick { get; set; }
        public string cross { get; set; }

        public int dailyMin { get; set; }
        public int dailyMax { get; set; }

        public int weeklyMin { get; set; }
        public int weeklyMax { get; set; }

        public int monthlyMin { get; set; }
        public int monthlyMax { get; set; }
    }
}
