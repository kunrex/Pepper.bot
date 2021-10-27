using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class DecoritiveItem : Item
    {
        public DecoritiveItem(string name, string description, string icon = ":grey_question:") : base(name, -1, description, UseType.Decoration | UseType.NonBuyable, icon)
        {

        }

        public DecoritiveItem(string name, string description, int price, string icon = ":grey_question:") : base(name, price, description, UseType.Decoration, icon)
        {

        }
    }
}
