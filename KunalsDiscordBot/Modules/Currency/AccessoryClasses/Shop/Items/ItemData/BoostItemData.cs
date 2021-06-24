using System;
namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public struct BoostItemData 
    {
        public enum BoostType
        {
            Luck,
            BankSpace,
            Invincibility
        }

        public readonly int MinBoost;
        public readonly int MaxBoost;

        public readonly int MinBoostTime;
        public readonly int MaxBoostTime;

        public readonly BoostType Type;

        public BoostItemData(BoostType type, int min, int max, int minTime = 0, int maxTime = 0)
        {
            MinBoost = min;
            MaxBoost = max;

            MinBoostTime = minTime;
            MaxBoostTime = maxTime;

            Type = type;
        }

        public int GetBoost() => new Random().Next(MinBoost, MaxBoost);
        public int GetBoostTime() => new Random().Next(MinBoostTime, MaxBoostTime);
        public bool IsTimed() => Type == BoostType.Invincibility || Type == BoostType.Luck;
    }
}
