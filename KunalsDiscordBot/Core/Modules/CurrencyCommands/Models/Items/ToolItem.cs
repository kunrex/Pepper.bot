using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public sealed class ToolItem : Item
    {
        public Tool Tool { get; private set; }

        public ToolItem(string name, int price, string description, UseType type, Tool tool) :base(name, price, description, type)
        {
            Tool = tool;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordClient client, DiscordChannel channel, DiscordMember member) => await Tool.Use(service, client, channel, member);
    }
}
