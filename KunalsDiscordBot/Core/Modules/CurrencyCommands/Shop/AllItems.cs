using System;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops
{
    public static partial class Shop
    {
        public static readonly Item Laptop = new PresenceItem("Laptop", 125, "Allows you to run the currency meme, code and game commands", UseType.Presence, new PresenceData(
           PresenceData.PresenceCommand.Meme | PresenceData.PresenceCommand.Game | PresenceData.PresenceCommand.Code, 100, 400));
        public static readonly Item BodyPillow = new PresenceItem("BodyPillow", 125, "Allows you to run the currency sleep command", UseType.Presence, new PresenceData(
           PresenceData.PresenceCommand.Sleep, 75, 300));
        public static readonly Item HuntingKit = new PresenceItem("Hunting Kit", 125, "Allows you to run the currency hunt and fish commands", UseType.Presence, new PresenceData(
           PresenceData.PresenceCommand.Hunt | PresenceData.PresenceCommand.Fish));
        public static readonly Item BankCard = new ToolItem("Bank Card", 125, "Provides extra bank space", UseType.Tool, Tool.BankCard);

        public static readonly Item Alcohol = new BoostItem("Alcohol", 10, "Boosts your luck!", UseType.Boost, new BoostData(Boost.Alcohol));
        public static readonly Item Landmine = new BoostItem("Landmine", 10, "If anyone tries to rob you when you have a landmine down they have a 50% chance of dieing", UseType.Boost, new BoostData(Boost.Landmine));
        public static readonly Item Padlock = new BoostItem("Padlock", 10, "If anyone tries to rob you when you have a padlock they instantly fail", UseType.Boost, new BoostData(Boost.Padlock));

        public static readonly Item Deer = new DecoritiveItem("Deer", "Just a deer", ":deer:");
        public static readonly Item Rabbit = new DecoritiveItem("Rabbit", "Just a Rabbit", ":rabbit2:");
        public static readonly Item Fish = new DecoritiveItem("Fish", "Just a fish", " :fish:");

        public static readonly Item Portrait = new DecoritiveItem("Portrait", "Peppers portrait", 20000, " :pepper:");

        public static readonly Item[] AllItems = new Item[] { Laptop, HuntingKit, BankCard, Alcohol, BodyPillow, Landmine, Padlock,
            Deer, Rabbit, Fish };

        public static readonly Item[] AllBuyableItems = new Item[] { Laptop, HuntingKit, BankCard, Alcohol, BodyPillow, Landmine, Padlock };

        public static readonly Item[] Animals = new Item[] { Deer, Rabbit, Fish };
    }
}
