using System.Linq;
using System.Collections.Generic;

using DSharpPlus.CommandsNext;

using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Core.Exceptions.CurrencyCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops
{
    public static class Shop
    {
        public static readonly Item Laptop = new PresenceItem("Laptop", 125, "Allows you to run the currency meme, code and game commands", UseType.Presence, new PresenceData(
            PresenceData.PresenceCommand.Meme | PresenceData.PresenceCommand.Game | PresenceData.PresenceCommand.Code, 100, 400
            ));

        public static readonly Item HuntingKit = new PresenceItem("Hunting Kit", 125, "Allows you to run the currency hunt and fish commands", UseType.Presence, new PresenceData(
           PresenceData.PresenceCommand.Hunt | PresenceData.PresenceCommand.Fish
            ));

        public static readonly Item BankCard = new ToolItem("Bank Card", 125, "Provides extra bank space", UseType.Tool, new ToolData(ToolData.ToolType.BankSpace, 350, 700
            ));

        public static readonly Item Alcohol = new BoostItem("Alcohol", 10, "Boosts your luck!", UseType.Boost, new BoostData(Boost.Alcohol));

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

        public static Item GetPresneceItem(CommandContext ctx)
        {
            var attribute = ctx.Command.CustomAttributes.FirstOrDefault(x => x is PresenceItemAttribute);

            if (attribute == null)
                throw new PresenceAttributeNotFoundException(ctx.Command.Name);

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
