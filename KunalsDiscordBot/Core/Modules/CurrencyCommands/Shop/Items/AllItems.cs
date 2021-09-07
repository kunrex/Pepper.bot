using System;
using System.Collections.Generic;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts;

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
        public static readonly Item BankCard = new ToolItem("Bank Card", 125, "Provides extra bank space", UseType.Tool, new ToolData(ToolData.ToolType.BankSpace, 350, 700));

        public static readonly Item Alcohol = new BoostItem("Alcohol", 10, "Boosts your luck!", UseType.Boost, new BoostData(Boost.Alcohol));
        public static readonly Item Landmine = new BoostItem("Landmine", 10, "Boosts your luck!", UseType.Boost, new BoostData(Boost.Landmine));
        public static readonly Item Padlock = new BoostItem("Padlock", 10, "Boosts your luck!", UseType.Boost, new BoostData(Boost.Padlock));

        public static readonly List<Item> AllItems = new List<Item> { Laptop, HuntingKit, BankCard, Alcohol, BodyPillow, Landmine, Padlock };
    }
}
