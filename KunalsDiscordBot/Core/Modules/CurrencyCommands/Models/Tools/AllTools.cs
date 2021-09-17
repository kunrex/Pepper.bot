using System;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Interfaces;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools
{
    public abstract partial class Tool : IDiscordUseableModel
    {
        public static readonly Tool BankCard = new BankCard("Bank Card", 300, 900);
    }
}
