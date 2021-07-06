using System;
namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public struct BoostData 
    {
        public enum BoostType
        {
            Luck,
            Invincibility
        }

        public readonly int MinBoost;
        public readonly int MaxBoost;

        public readonly int MinBoostTime;
        public readonly int MaxBoostTime;

        public readonly BoostType Type;

        public BoostData(BoostType type, int min, int max, int minTime, int maxTime)
        {
            MinBoost = min;
            MaxBoost = max;

            MinBoostTime = minTime;
            MaxBoostTime = maxTime;

            Type = type;
        }

        public int GetBoost() => new Random().Next(MinBoost, MaxBoost);
        public int GetBoostTime() => new Random().Next(MinBoostTime, MaxBoostTime);
    }
}
