using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Tools;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public sealed class ToolItem : Item
    {
        public Tool Tool { get; private set; }

        public ToolItem(string name, int price, string description, UseType type, Tool tool, string icon = ":grey_question:") :base(name, price, description, type, icon)
        {
            Tool = tool;
        }

        public async override Task<UseResult> Use(Profile profile, IProfileService profileService)
        {
            var result = await Tool.Use(profile, profileService);
            await profileService.AddOrRemoveItem(profile, Name, -1);

            return result;
        }
    }
}
