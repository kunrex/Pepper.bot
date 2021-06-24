using System;
using KunalsDiscordBot.Modules.Currency.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    public class BoostItem : Item
    {
        public BoostItemData Data { get; private set; }

        public BoostItem(string name, int price, string description, UseType type, BoostItemData data) : base(name, price, description, type)
        {
            Name = name;
            Price = price;
            Description = description;

            Type = type;
            SellingPrice = Price / 2;
            Data = data;
        }

        public override UseResult Use()
        {
            int boost = Data.GetBoost();
            int time = Data.GetBoostTime();

            return new UseResult
            {
                usableItem = true,
                BoostName = Data.Type.ToString(),

                isTimed = Data.IsTimed(),
                BoostValue = boost,
                BooseTime = time,

                boostType = Data.Type
            };
        }
    }

    public partial struct UseResult
    {
        public bool isTimed { get; set; }

        public int BoostValue { get; set; }
        public int BooseTime { get; set; }

        public string BoostName { get; set; }
        public string BoostStart { get; set; }

        public BoostItemData.BoostType boostType { get; set; }
    }
}
