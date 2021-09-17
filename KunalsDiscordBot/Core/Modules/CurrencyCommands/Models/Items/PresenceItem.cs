using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class PresenceItem : Item
    {
        public PresenceData Data { get; private set; }
         
        public PresenceItem(string name, int price, string description, UseType type, PresenceData data) : base(name, price, description, type)
        {
            Data = data;
        }

        public override Task<UseResult> Use(IProfileService service, DiscordMember member) => Task.FromResult(new UseResult { useComplete = false, message = "You can't use this item??" });
    }
}
