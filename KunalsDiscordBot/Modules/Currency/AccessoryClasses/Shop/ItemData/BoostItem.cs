using System;
namespace KunalsDiscordBot.Modules.Currency.Shops.Items
{
    public struct BoostItem 
    {
        public enum BoostType
        {
            Luck,
            BankSpace
        }

        public int minBoost { get; set; }
        public int maxBoost { get; set; }
    }
}
