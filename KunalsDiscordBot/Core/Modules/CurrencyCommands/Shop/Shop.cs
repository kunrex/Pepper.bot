using System.Linq;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;

using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Core.Exceptions.CurrencyCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops
{
    public static partial class Shop
    {
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

        public static Item GetPresneceItem(Command command)
        {
            var attribute = command.CustomAttributes.FirstOrDefault(x => x is PresenceItemAttribute);

            if (attribute == null)
                throw new PresenceAttributeNotFoundException(command.Name);

            var casted = (PresenceItemAttribute)attribute;

            var items = AllItems.Where(x => x is PresenceItem).ToList();
            var castedItems = items.ConvertAll(x => (PresenceItem)x);
            return castedItems.Find(x => (x.Data.allowedCommands & casted.commandNeeded) == casted.commandNeeded);
        }
    }

    public struct BuyResult
    {
        public bool completed { get; set; }
        public string message { get; set; }
        public Item item { get; set; }
    }
}
