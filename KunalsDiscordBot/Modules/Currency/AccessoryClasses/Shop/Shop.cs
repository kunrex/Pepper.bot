using System;
using DiscordBotDataBase.Dal.Models.Profile;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using KunalsDiscordBot.Modules.Currency.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency.Shops
{
    public static class Shop
    {
        public static readonly Item Laptop = new PresenceItem("Laptop", 125, "Allows you to run the currency meme, code and game commands", UseType.Presence, new PresenceItemData(
            PresenceItemData.PresenceCommand.Meme | PresenceItemData.PresenceCommand.Game | PresenceItemData.PresenceCommand.Code
            ));

        public static readonly Item HuntingKit = new PresenceItem("Hunting Kit", 125, "Allows you to run the currency hunt and fish commands", UseType.Presence, new PresenceItemData(
           PresenceItemData.PresenceCommand.Hunt | PresenceItemData.PresenceCommand.Fish
           ));

        public static readonly Item BankCard = new BoostItem("Bank Card", 125, "Provides extra bank space", UseType.BoostMoney, new BoostItemData(
            BoostItemData.BoostType.BankSpace, 350, 700
          ));

        public static readonly Item Alcohol = new BoostItem("Alcohol", 10, "Boosts your luck!", UseType.BoostLuck, new BoostItemData(
           BoostItemData.BoostType.Luck, 10, 20
         ));

        public static readonly List<Item> AllItems = new List<Item> { Laptop, HuntingKit, BankCard, Alcohol };

        public static BuyResult Buy(string itemName, int quantity, in Profile profile)
        {
            var item = GetItem(itemName);

            if (item == null)
                return new BuyResult { completed = false, message = "The given item not found" };

            if(profile.Coins >= item.Price * quantity)
            {
                return new BuyResult { completed = true, message = $"Successfully bought {quantity} {itemName}(s) for {quantity * item.Price}", item = item};
            }
            else
                return new BuyResult { completed = false, message = "You don't have enough coins to buy this item" };
        }

        public static Item GetItem(string itemName) => AllItems.Find(x => x.Name.ToLower().Replace(" ", "") == itemName.ToLower().Replace(" ",""));
    }

    public struct BuyResult
    {
        public bool completed { get; set; }
        public string message { get; set; }
        public Item item { get; set; }
    }
}
