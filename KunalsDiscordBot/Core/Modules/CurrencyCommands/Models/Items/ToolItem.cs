using System.Threading.Tasks;

using DSharpPlus.Entities;

using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class ToolItem : Item
    {
        public ToolData Data;

        public ToolItem(string name, int price, string description, UseType type, ToolData data) :base(name, price, description, type)
        {
            Data = data;
        }

        public async override Task<UseResult> Use(IProfileService service, DiscordMember member)
        {
            int boost = Data.GetBoost();

            switch(Data.Type)
            {
                 case ToolData.ToolType.BankSpace:
                    await service.ModifyProfile(member.Id, x => x.CoinsBankMax += boost);
                    break;
            }
            await service.AddOrRemoveItem(member.Id, Name, -1).ConfigureAwait(false);

            return new UseResult
            {
                useComplete = true,
                message = $"{member.Mention}, Increase {Data.Type} by {boost}"
            };
        }
    }
}
