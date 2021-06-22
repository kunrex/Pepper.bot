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
        public static readonly Item Laptop = new Item("Laptop", 125, "Allows you to run the currency meme, code and game commands",
            UseType.Presence, new PresenceItem(PresenceItem.PresenceCommand.Code | PresenceItem.PresenceCommand.Game | PresenceItem.PresenceCommand.Meme));

        public static readonly Item HuntingKit = new Item("Hunting Kit", 125, "Allows you to use the currency hunt and currency fish commands",
            UseType.Presence, new PresenceItem(PresenceItem.PresenceCommand.Hunt | PresenceItem.PresenceCommand.Fish));

        public static readonly List<Item> AllItems = new List<Item> { Laptop, HuntingKit };

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
