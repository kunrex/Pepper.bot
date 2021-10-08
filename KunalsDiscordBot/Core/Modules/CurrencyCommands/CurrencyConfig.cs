using System;
namespace KunalsDiscordBot.Core.Modules.CurrencyCommands
{
    public class CurrencyModuleData
    {
        public string CoinsEmoji { get; set; }
        public int ThumbnailSize { get; set; }

        public string Tick { get; set; }
        public string Cross { get; set; }

        public int DailyMin { get; set; }
        public int DailyMax { get; set; }

        public int WeeklyMin { get; set; }
        public int WeeklyMax { get; set; }

        public int MonthlyMin { get; set; }
        public int MonthlyMax { get; set; }

        public int XPMultiplier { get; set; }
        public float LevelConstant { get; set; }
        public int CoinMultiplier { get; set; }

        public string[] Memes { get; set; }
    }
}
