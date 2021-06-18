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
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    [Description("A currency system!")]
    public class CurrencyCommands : BaseCommandModule
    {
        private readonly IProfileService service;
        private const string coinsEmoji = ":coin:";

        public CurrencyCommands(IProfileService _service)
        {
            service = _service;
        }
        
        [Command("profile")]
        [Description("Gets the profile of he user")]
        public async Task GetProfile(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;
            bool sameMember = true;

            if (member != ctx.Member)
                sameMember = false;

            var profile = await service.GetProfile(member, sameMember);

            if(profile == null)
            {
                var nullEmbed = new DiscordEmbedBuilder
                {
                    Title = member.Username,
                    Description = "Does not have an account",
                    Color = DiscordColor.Gold
                };

                await ctx.Channel.SendMessageAsync(nullEmbed).ConfigureAwait(false);

                return;
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = member.AvatarUrl,
                Height = 50,
                Width = 50
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = profile.Name,
                Thumbnail = thumbnail,
                Color = DiscordColor.Gold
            };

            embed.AddField("ID: ", profile.DiscordUserID.ToString());
            embed.AddField("Coins: ", $"{profile.Coins} {coinsEmoji}");
            embed.AddField("Bank: ", $"{profile.CoinsBank}");

            int level = profile.XP;

            embed.AddField("Level: ", $"{level}");

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("AddCoins")]
        public async Task AddCoins(CommandContext ctx)
        {
            bool completed = await service.ChangeCoins(ctx.Member, 10);

            await ctx.Channel.SendMessageAsync(completed.ToString());
        }

        [Command("AddItem")]
        public async Task AddItem(CommandContext ctx)
        {
            bool completed = await service.AddOrRemoveItem(ctx.Member, "testItem", 10);

            await ctx.Channel.SendMessageAsync(completed.ToString());
        }

        [Command("GetItem")]
        public async Task GetItem(CommandContext ctx)
        {
            ItemDBData data = await service.GetItem(ctx.Member, "testItem");

            await ctx.Channel.SendMessageAsync(data.Count.ToString());
        }
    }
}
