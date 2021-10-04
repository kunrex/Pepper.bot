using System;
using System.Threading.Tasks;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shop
{
    public class StockMarket
    {
        private const int lowestStockPrices = -20, highestStockPrices = 50, minimumResetTime = 2, maximumResetTime = 5;

        private static StockMarket instance;
        public static StockMarket Instance
        {
            get
            {
                if (instance == null)
                    instance = new StockMarket();

                return instance;
            }
        }

        private int currentStockPrice;
        public int CurrentStockPrice { get => currentStockPrice; }

        private StockMarket()
        {
            currentStockPrice = new Random().Next(lowestStockPrices, highestStockPrices);

            Task.Run(() => instance.StockCrash());
        }

        public int CalculatePrice(int price) => (int)(currentStockPrice / 100f * price);

        private async Task StockCrash()
        {
            var rng = new Random();

            while (true)
            {
                var span = TimeSpan.FromHours(rng.Next(minimumResetTime, maximumResetTime));
                await Task.Delay(span);

                currentStockPrice = rng.Next(lowestStockPrices, highestStockPrices);
            }
        }
    }
}
