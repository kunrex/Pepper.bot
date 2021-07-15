//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

//Custom name spaces
using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Modules.Currency.Jobs;
using KunalsDiscordBot.Modules.Currency.Shops;
using KunalsDiscordBot.DialogueHandlers;
using KunalsDiscordBot.DialogueHandlers.Steps;

using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Services;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    [Description("A currency system!")]
    public class CurrencyCommands : BaseCommandModule
    {
        private readonly IProfileService service;
        private const string coinsEmoji = ":coin:";

        private static readonly DiscordColor Color = typeof(CurrencyCommands).GetCustomAttribute<Decor>().color;
        private static readonly int ThumbnailSize = 30;

        private static readonly string tick = ":white_check_mark:";
        private static readonly string cross = ":x:";

        private static readonly int dailyMin = 200;
        private static readonly int dailyMax = 800;

        private static readonly int weeklyMin = 500;
        private static readonly int weeklyMax = 1200;

        private static readonly int monthlyMin = 1000;
        private static readonly int monthlyMax = 2500;

        public CurrencyCommands(IProfileService _service) => service = _service;

        [Command("profile")]
        [Description("Gets the profile of he user")]
        public async Task GetProfile(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;
            bool sameMember = true;

            if (member.Id != ctx.Member.Id)
                sameMember = false;

            var profile = await service.GetProfile(member.Id, member.Username, sameMember);

            if(profile == null)
            {
                var nullEmbed = new DiscordEmbedBuilder
                {
                    Title = member.Username,
                    Description = "Does not have an account",
                    Color = Color
                };

                await ctx.Channel.SendMessageAsync(nullEmbed).ConfigureAwait(false);

                return;
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = member.AvatarUrl,
                Height = ThumbnailSize,
                Width = ThumbnailSize
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = profile.Name,
                Thumbnail = thumbnail,
                Color = DiscordColor.Gold
            };

            embed.AddField("ID: ", profile.DiscordUserID.ToString());
            embed.AddField("Coins: ", $"{profile.Coins} {coinsEmoji}");
            embed.AddField("Bank: ", $"{profile.CoinsBank} {coinsEmoji} (max: {profile.CoinsBankMax})");

            int level = service.GetLevel(profile);
            embed.AddField("Level: ", $"{level}");
            embed.AddField("Job: ", profile.Job);

            string boosts = string.Empty;
            int index = 1;
            foreach (var boost in await service.GetBoosts(ctx.Member.Id))
            {
                boosts += $"{index}. {boost.BoosteName}\n";
                index++;
            }
            embed.AddField("Boosts:\n", boosts == string.Empty ? "None" : boosts);

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("Deposit")]
        [Aliases("dep")]
        [Description("Deposits Money into the bank")]
        public async Task Deposit(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var coins = profile.Coins;
            var coinsBank = profile.CoinsBank;
            var maxBank = profile.CoinsBankMax;
            var difference = maxBank - coinsBank;

            if (coinsBank == maxBank)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} your bank is full");
                return;
            }
            else if (coins <= 0)
            {
                await ctx.Channel.SendMessageAsync($"your wallet is empty");
                return;
            }


            if (amount.ToLower().Equals("max"))
            {
                var toDep = System.Math.Min(difference, coins);

                await service.ChangeCoinsBank(ctx.Member.Id, toDep);
                await service.ChangeCoins(ctx.Member.Id, -toDep);

                await ctx.Channel.SendMessageAsync($"Desposited max({toDep} {coinsEmoji})");
            }
            else if (int.TryParse(amount, out int x))
            {
                var toDep = int.Parse(amount);

                if(toDep > difference)
                {
                    await ctx.Channel.SendMessageAsync("Don't try to break the bot");
                    return;
                }

                await service.ChangeCoinsBank(ctx.Member.Id, toDep);
                await service.ChangeCoins(ctx.Member.Id, -toDep);

                await ctx.Channel.SendMessageAsync($"Deposited {toDep} {coinsEmoji}");
            }
            else
                await ctx.Channel.SendMessageAsync("Don't try to break the bot");
        }

        [Command("Withdraw")]
        [Aliases("with")]
        [Description("Withdraws Money into the bank")]
        public async Task Withdraw(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var coins = profile.Coins;
            var coinsBank = profile.CoinsBank;

            if (amount.ToLower().Equals("max"))
            {
                await service.ChangeCoinsBank(ctx.Member.Id, -coinsBank);
                await service.ChangeCoins(ctx.Member.Id, coinsBank);

                await ctx.Channel.SendMessageAsync($"Withdrawed max({coinsBank} {coinsEmoji})");
            }
            else if (int.TryParse(amount, out int x))
            {
                var toWith = int.Parse(amount);

                if (toWith > coinsBank)
                {
                    await ctx.Channel.SendMessageAsync("Don't try to break the bot");
                    return;
                }

                await service.ChangeCoinsBank(ctx.Member.Id, -toWith);
                await service.ChangeCoins(ctx.Member.Id, toWith);

                await ctx.Channel.SendMessageAsync($"Withdrawed {toWith} {coinsEmoji}");
            }
            else
                await ctx.Channel.SendMessageAsync("Don't try to break the bot");
        }

        [Command("Lend")]
        [Description("Lend another user money")]
        public async Task Lend(CommandContext ctx, DiscordMember member, string amount)
        {
            if (member.Id == ctx.Member.Id)
            {
                await ctx.Channel.SendMessageAsync("You can't lend money to your self genius").ConfigureAwait(false);
                return;
            }

            var other = service.GetProfile(member.Id, member.Username, false);
            if(other == null)
            {
                await ctx.Channel.SendMessageAsync($"{member.Mention} does not have a profile").ConfigureAwait(false);
                return;
            }

            if (amount.Equals("max"))
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

                if (profile.Coins <= 0)
                {
                    await ctx.Channel.SendMessageAsync("You're broke dude...").ConfigureAwait(false);
                    return;
                }

                var coins = profile.Coins;
                await service.ChangeCoins(ctx.Member.Id, -coins);
                await service.ChangeCoins(member.Id, coins);

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} lended {coins} {coinsEmoji} to {member.Mention}").ConfigureAwait(false);
            }
            else if (int.TryParse(amount, out int x))
            {
                var coins = int.Parse(amount);

                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

                if (profile.Coins <= 0)
                {
                    await ctx.Channel.SendMessageAsync("You're broke dude...").ConfigureAwait(false);
                    return;
                }
                else if (profile.Coins < coins)
                {
                    await ctx.Channel.SendMessageAsync("You can't lend money you don't have").ConfigureAwait(false);
                    return;
                }

                await service.ChangeCoins(ctx.Member.Id, -coins);
                await service.ChangeCoins(member.Id, coins);

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} lended {coins} {coinsEmoji} to {member.Mention}").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Don't try to break the bot").ConfigureAwait(false);
                return;
            }
        }

        [Command("Daily")]
        [Description("Log in daily to collect some coins")]
        public async Task Daily(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username).ConfigureAwait(false); ;

            if (DateTime.TryParse(profile.PrevLogDate, out var x))
            {
                var prevDate = DateTime.Parse(profile.PrevLogDate);

                var timeSpan = DateTime.Now - prevDate;
                if (timeSpan.TotalDays < 1)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Chill out",
                        Description = $"Its only been {timeSpan.TotalHours} hours since you've used this command",
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Height = ThumbnailSize,
                            Width = ThumbnailSize,
                            Url = ctx.Member.AvatarUrl
                        },
                        Color = Color
                    }).ConfigureAwait(false);

                    return;
                }
            }

            var coins = new Random().Next(dailyMin, dailyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); 
            await service.ChangeLogDate(ctx.Member.Id, 0, DateTime.Now).ConfigureAwait(false); 

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Daily Coins",
                Description = $"{coins} {coinsEmoji}, Here are your coins for the day.",
                Color = Color,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = ctx.Member.AvatarUrl
                },
            }).ConfigureAwait(false);
        }

        [Command("Weekly")]
        [Description("Log in weekly to collect some coins")]
        public async Task Weekly(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username).ConfigureAwait(false); ;

            if (DateTime.TryParse(profile.PrevMonthlyLogDate, out var x))
            {
                var prevDate = DateTime.Parse(profile.PrevMonthlyLogDate);

                var timeSpan = DateTime.Now - prevDate;
                if (timeSpan.TotalDays < 7)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Chill out",
                        Description = $"Its only been {timeSpan.TotalDays} days since you've used this command",
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Height = ThumbnailSize,
                            Width = ThumbnailSize,
                            Url = ctx.Member.AvatarUrl
                        },
                        Color = Color
                    }).ConfigureAwait(false);

                    return;
                }
            }

            var coins = new Random().Next(weeklyMin, weeklyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); 
            await service.ChangeLogDate(ctx.Member.Id, 1, DateTime.Now).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Weekly Coins",
                Description = $"{coins} {coinsEmoji}, Here are your coins for the week.",
                Color = Color,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = ctx.Member.AvatarUrl
                },
            }).ConfigureAwait(false);
        }

        [Command("Monthly")]
        [Description("Log in monthly to collect some coins, for simplcity a month is averegaed to 30 days")]
        public async Task Monthly(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username).ConfigureAwait(false); ;

            if (DateTime.TryParse(profile.PrevMonthlyLogDate, out var x))
            {
                var prevDate = DateTime.Parse(profile.PrevMonthlyLogDate);

                var timeSpan = DateTime.Now - prevDate;
                if (timeSpan.TotalDays < 30)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Chill out",
                        Description = $"Its only been {timeSpan.TotalDays} days since you've used this command",
                        Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                        {
                            Height = ThumbnailSize,
                            Width = ThumbnailSize,
                            Url = ctx.Member.AvatarUrl
                        },
                        Color = Color
                    }).ConfigureAwait(false);

                    return;
                }
            }

            var coins = new Random().Next(monthlyMin, monthlyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); ;
            await service.ChangeLogDate(ctx.Member.Id, 2, DateTime.Now).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Monthly Coins",
                Description = $"{coins} {coinsEmoji}, Here are your coins for the monthly.",
                Color = Color,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = ctx.Member.AvatarUrl
                },
            }).ConfigureAwait(false);
        }

        [Command("SafeMode")]
        [Aliases("sm")]
        [Description("Toggles safe mode")]
        public async Task SafeMode(CommandContext ctx)
        {
            bool completed = await service.ToggleSafeMode(ctx.Member.Id).ConfigureAwait(false);
            if(!completed)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "You don't have an account",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Height = ThumbnailSize,
                        Width = ThumbnailSize,
                        Url = ctx.Member.AvatarUrl
                    },
                    Color = Color
                });

                return;
            }

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            await ctx.RespondAsync($"Safe mode set to {profile.SafeMode == 1}").ConfigureAwait(false);
        }

        [Command("JobList")]
        [Description("can't remain unemployed can you?")]
        public async Task JobList(CommandContext ctx)
        {
            var jobs = Job.AllJobs;

            string description = string.Empty;
            int level = await service.GetLevel(ctx.Member.Id);

            for (int i = 0; i < jobs.Length; i++)
            {
                var job = jobs[i];
                description += $"{i + 1}. {(level < job.minLvlNeeded ? cross : tick)} **{job.Name}** {job.Emoji}\n `Min Level:` {job.minLvlNeeded}\n `Avg wage:` {((job.SucceedMax + job.SucceedMin) / 2)}\n\n";
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.Member.AvatarUrl,
                Width = ThumbnailSize,
                Height = ThumbnailSize
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Job List",
                Description = description,
                Color = Color,
                Thumbnail = thumbnail
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Hire")]
        [Description("Apply for a job")]
        public async Task Hire(CommandContext ctx, string jobName)
        {
            var job = Job.AllJobs.FirstOrDefault(x => x.Name.ToLower() == jobName.ToLower());

            if (job == null)
                await ctx.Channel.SendMessageAsync("The given job was not found");

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var level = service.GetLevel(profile);

            if(!profile.Job.Equals("None"))
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} you already have a job, use the resign command to resign");
                return;
            }
            else if(level < job.minLvlNeeded)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} the min level needed for the job is {job.minLvlNeeded}, you're level is {level}");
                return;
            }

            bool completed = await service.ChangeJob(ctx.Member.Id, job.Name);

            if (!completed)
            {
                await ctx.Channel.SendMessageAsync("You need a profile to run this command, if you have one something must have gone wrong.").ConfigureAwait(false);
                return;
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.Member.AvatarUrl,
                Width = ThumbnailSize,
                Height = ThumbnailSize
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Cogratulations",
                Description = $"{ctx.Member.Mention} you have been hired. \nUse the work command to earn some money.",
                Color = Color,
                Thumbnail = thumbnail
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Resign")]
        [Description("Resign from a job")]
        public async Task Resign(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var prevJob = profile.Job;

            if (profile == null)
                await ctx.Channel.SendMessageAsync("You need a profile to run this command");

            if (profile.Job.Equals("None"))
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} you're not employed");
                return;
            }

            bool completed = await service.ChangeJob(ctx.Member.Id, "None");

            if (!completed)
            {
                await ctx.Channel.SendMessageAsync("Something went wrong").ConfigureAwait(false);
                return;
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.Member.AvatarUrl,
                Width = ThumbnailSize,
                Height = ThumbnailSize
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Resigned",
                Description = $"{ctx.Member.Mention} has resigned from being a {prevJob}",
                Color = Color,
                Thumbnail = thumbnail
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        Func<int, int, int> GenerateRandom = (int min, int max) => new Random().Next(min, max);

        [Command("Work")]
        [Description("Want to make some money?")]
        public async Task Work(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            if (profile.Job.Equals("None"))
            {
                await ctx.Channel.SendMessageAsync("You don't have a job, use the joblist command to list them");
                return;
            }

            var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Height = ThumbnailSize,
                Width = ThumbnailSize,
                Url = ctx.Member.AvatarUrl
            };

            var steps = await job.GetWork(Color, thumbnail);

            if (DateTime.TryParse(profile.PrevWorkDate, out var x))
            {
                var prevDate = DateTime.Parse(profile.PrevWorkDate);

                var timeSpan = DateTime.Now - prevDate;
                if (timeSpan.TotalHours < job.CoolDown)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Chill out",
                        Description = $"Its only been {timeSpan} since your last shift, {job.CoolDown} hours is the minimum time.",
                        Thumbnail = thumbnail,
                        Color = Color
                    }).ConfigureAwait(false);

                    return;
                }
            }

            DialogueHandlerConfig config = new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = true
            };

            DialogueHandler handler = new DialogueHandler(config, steps); 
            var completed = await handler.ProcessDialogue();

            int money = completed?  GenerateRandom(job.SucceedMin, job.SucceedMax) : GenerateRandom(job.FailMin, job.FailMax);

            var completedEmbed = new DiscordEmbedBuilder
            {
                Title = completed ? "Good Job" : "Time Out",
                Description = completed ? $"You recieve {money} coins for a job well done" : $"You recieve only {money} coins",
                Color = Color,
                Thumbnail = thumbnail
            };

            await service.ChangeCoins(ctx.Member.Id, money);
            await service.ChangeLogDate(ctx.Member.Id, 3, DateTime.Now);

            await ctx.Channel.SendMessageAsync(embed: completedEmbed).ConfigureAwait(false);
        }

        [Command("Shop")]
        [Description("Diaplays items in the shop")]
        public async Task ShowShop(CommandContext ctx)
        {
            var items = Shop.AllItems;

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Width = ThumbnailSize,
                Height = ThumbnailSize,
                Url = ctx.Member.AvatarUrl
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Shop",
                Thumbnail = thumbnail,
                Color = Color
            };
            int index = 0;

            foreach (var item in items)
                embed.AddField($"{++index}. {item.Name}\n", $" Description: {item.Description}\nPrice: {item.Price}\n\n");

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Shop")]
        [Description("Gives info about an item")]
        public async Task ShowShop(CommandContext ctx, [RemainingText] string itemName)
        {
            var item = Shop.GetItem(itemName);
            var proilfeItem = await service.GetItem(ctx.Member.Id, item.Name);

            if (item == null)
            {
                await ctx.Channel.SendMessageAsync("The item doesn't exist??");
                return;
            }

            var emoji = DiscordEmoji.FromName(ctx.Client, ":coin:");

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Width = ThumbnailSize,
                Height = ThumbnailSize,
                Url = emoji.Url
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = item.Name,
                Description = item.Description,
                Thumbnail = thumbnail,
                Color = Color
            };

            embed.AddField("Price: ", item.Price.ToString(), true);
            embed.AddField("Owned: ", proilfeItem.Count.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Buy")]
        [Description("Buy an item")]
        public async Task BuyItem(CommandContext ctx, int quantity, [RemainingText]string itemName)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var result = Shop.Buy(itemName, quantity, in profile);
            if (!result.completed)
                await ctx.Channel.SendMessageAsync(result.message).ConfigureAwait(false);
            else
            {
                await service.ChangeCoins(ctx.Member.Id , - result.item.Price * quantity);
                await service.AddOrRemoveItem(ctx.Member.Id, result.item.Name, quantity);

                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = ctx.Member.AvatarUrl
                };

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Purchase Successful",
                    Description = result.message,
                    Thumbnail = thumbnail,
                    Color = Color
                };

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
        }

        [Command("Sell")]
        [Description("Sell an item")]
        public async Task Sell(CommandContext ctx, int quantity, [RemainingText] string itemName)
        {
            var result = Shop.GetItem(itemName);
            if (result == null)
                await ctx.Channel.SendMessageAsync("The item doesn't exist??");

            var item = service.GetItem(ctx.Member.Id, result.Name);           
            if (item == null)
                await ctx.Channel.SendMessageAsync("You don't have this item??").ConfigureAwait(false);
            else
            {
                await service.ChangeCoins(ctx.Member.Id, result.SellingPrice * quantity);
                await service.AddOrRemoveItem(ctx.Member.Id, result.Name, -quantity);

                var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Height = ThumbnailSize,
                    Width = ThumbnailSize,
                    Url = ctx.Member.AvatarUrl
                };

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Sold item",
                    Description = $"Successfuly sold {quantity} {result.Name}(s) for {quantity * result.SellingPrice}",
                    Thumbnail = thumbnail,
                    Color = Color
                };

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
        }

        [Command("Gift")]
        [Description("Gift items to other members")]
        public async Task Gift(CommandContext ctx, DiscordMember member, int number, [RemainingText]string itemName)
        {
            var other = service.GetProfile(member.Id, member.Username, false);
            if (other == null)
            {
                await ctx.Channel.SendMessageAsync($"{member.Mention} does not have a profile").ConfigureAwait(false);
                return;
            }

            var result = Shop.GetItem(itemName);
            if (result == null)
                await ctx.Channel.SendMessageAsync("The item doesn't exist??");

            var item = service.GetItem(ctx.Member.Id, result.Name);
            if (item == null)
                await ctx.Channel.SendMessageAsync("You don't have this item??").ConfigureAwait(false);
            else
            {
                await service.AddOrRemoveItem(ctx.Member.Id, result.Name, -number);
                await service.AddOrRemoveItem(member.Id, result.Name, number);

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} gave {number} {result.Name}(s) to {member.Mention}").ConfigureAwait(false);
            }
        }

        [Command("Inventory")]
        [Description("Shows your inventory")]
        public async Task ShowInventory(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            var items = await service.GetItems(member.Id);

            string descripton = string.Empty;

            int index = 1;
            foreach(var item in items)
            {
                descripton += $"{index}. {item.Name}\n  Quantity: {item.Count}\n\n";
                index++;
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Height = ThumbnailSize,
                Width = ThumbnailSize,
                Url = member.AvatarUrl
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member.Username}'s Inventory",
                Description = descripton,
                Color = Color,
                Thumbnail = thumbnail
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Use")]
        [Description("Shows your inventory")]
        public async Task Use(CommandContext ctx, [RemainingText] string itemName)
        {
            var item = Shop.GetItem(itemName);
            if(item == null)
            {
                await ctx.Channel.SendMessageAsync("This item doesn't exist??").ConfigureAwait(false);
                return;
            }

            var itemData = await service.GetItem(ctx.Member.Id, item.Name);
            if (itemData == null)
            {
                await ctx.Channel.SendMessageAsync("You don't have this item??").ConfigureAwait(false);
                return;
            }

            var useResult = await item.Use(service, ctx.Member);
            if (!useResult.useComplete)
            {
                await ctx.Channel.SendMessageAsync(useResult.message).ConfigureAwait(false);
                return;
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Item Successfully Used",
                Description = useResult.message,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = $"Item: {itemData.Name}"
                },
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = ctx.Member.AvatarUrl,
                    Height = ThumbnailSize,
                    Width = ThumbnailSize
                }
            }).ConfigureAwait(false);
        }

        [Command("Meme")]
        [Description("The currency meme command")]
        [PresenceItem(Shops.Items.PresenceData.PresenceCommand.Meme)]
        public async Task Meme(CommandContext ctx)
        {
            var itemNeed = Shop.GetPresneceItem(ctx);

            var itemData = await service.GetItem(ctx.Member.Id, itemNeed.Name).ConfigureAwait(false);
            if(itemData == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = $"You need a {itemNeed.Name} to run this command, you have 0",
                    Footer = BotService.GetEmbedFooter($"User: {ctx.Member.Username}"),
                    Color = Color
                }).ConfigureAwait(false);

                return;
            }

            var casted = itemNeed as PresenceItem;
            var reward = casted.Data.GetReward();

            await service.ChangeCoins(ctx.Member.Id, reward);

            string descripion = reward switch
            {
                0 => "Well no one liked your meme lmao, you get 0 coins",
                var y when y < casted.Data.maxReward / 2 => $"Your meme got a half decent reponse online, good job. You get {reward} coins",
                _ => $"Damn your meme is **Trending**, gotta say im impressed. Here you go, {reward} coins"
            };

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Memes",
                Description = descripion,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, ThumbnailSize),
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.Username}"),
                Color = Color
            }).ConfigureAwait(false);
        }
    }
}
