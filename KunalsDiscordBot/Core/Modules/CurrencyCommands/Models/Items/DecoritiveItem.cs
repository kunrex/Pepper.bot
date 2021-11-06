using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class DecoritiveItem : Item
    {
        public DecoritiveItem(string name, int price, string description, UseType useType = UseType.Decoration, string icon = ":grey_question:") : base(name, price, description, useType, icon)
        {

        }
    }
}
