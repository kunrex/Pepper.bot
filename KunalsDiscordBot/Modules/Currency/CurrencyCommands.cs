//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Attributes;
using DiscordBotDataBase.Dal;
using DiscordBotDataBase.Dal.Models.Items;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    public class CurrencyCommands : BaseCommandModule
    {
        private readonly DataContext context;

        public CurrencyCommands(DataContext _context)
        {
            context = _context;
        }

        [Command("AddItem")]
        [Description("Test command for adding items")]
        public async Task AddItem(CommandContext ctx, string name)
        {
            try
            {
                await context.Items.AddAsync(new Item { Name = name, Description = "just a test item" });
                await context.SaveChangesAsync().ConfigureAwait(false);

                await ctx.Channel.SendMessageAsync("Added Item");
            }
            catch(Exception e)
            {
                await ctx.Channel.SendMessageAsync($"{e.Message}, inner exception\n {e.InnerException}");
            }           
        }

        [Command("GetItem")]
        [Description("Gets Item")]
        public async Task RemoveItem(CommandContext ctx, string name)
        {
            var item = await context.Items.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());

            if (item != null)
                await ctx.Channel.SendMessageAsync(item.Description);
        }
    }
}
