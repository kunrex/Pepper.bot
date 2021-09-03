using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;

using KunalsDiscordBot.Core;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.DialogueHandlers;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules.CurrencyCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    [Description("A currency system!"), ConfigData(ConfigValueSet.Currency)]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AccessChannels)]
    public sealed partial class CurrencyCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IProfileService service;
        private readonly CurrencyModuleData data;

        private bool ExecutionRewards { get; set; } = false;

        public CurrencyCommands(IProfileService _service, PepperConfigurationManager configurationManager, IModuleService moduleService)
        {
            service = _service;
            data = configurationManager.currenyConfig;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Currency];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var requireProfile = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireProfileAttribute) != null;
            if(requireProfile)
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username, false);

                if (profile == null)
                {
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Description = "You need a profile to run this command",
                        Color = ModuleInfo.Color
                    }.WithFooter("Use the `currency profile` command to create a profile")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            var jobCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireJobAttribute);
            if(jobCheck != null)
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
                if (profile.Job.Equals("None") && ((RequireJobAttribute)jobCheck).require)
                {
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Description = "You need a job to run this command",
                        Color = ModuleInfo.Color
                    }.WithFooter("Use the \"currency hire\" command to get a job")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
                else if(!profile.Job.Equals("None") && !((RequireJobAttribute)jobCheck).require)
                {
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Description = "You should not have a job to run this command",
                        Color = ModuleInfo.Color
                    }.WithFooter("Use the `currency resign` command to resign from your job")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            var checkWorkDate = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckWorkDateAttribute) != null;
            if(checkWorkDate)
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
                var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);

                if (DateTime.TryParse(profile.PrevWorkDate, out var x))
                {
                    var prevDate = DateTime.Parse(profile.PrevWorkDate);

                    var timeSpan =  DateTime.Now - prevDate;
                    if (timeSpan.TotalHours < (profile.Job == "None" ? Job.resignTimeSpan.TotalHours : job.CoolDown))
                    {
                        var timeString = $"{timeSpan.Days} dyas, {timeSpan.Hours} hours, {timeSpan.Minutes}, {timeSpan.Seconds} seconds";

                        await ctx.RespondAsync(new DiscordEmbedBuilder
                        {
                            Title = "Chill out",
                            Description = profile.Job == "None" ? $"You just resigned and its only been {timeString}, have to wait {Job.resignTimeSpan} hours before you can apply again" : $"Its only been {timeString} since your last shift, {job.CoolDown} hours is the minimum time.",
                            Color = ModuleInfo.Color
                        }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);

                        throw new CustomCommandException();
                    }
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        public async override Task AfterExecutionAsync(CommandContext ctx)
        {
            if (!ExecutionRewards)
                return;

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username, false);
            if (profile == null)
                return;

            var provideXP = ctx.Command.CustomAttributes.FirstOrDefault(x => x is NonXPCommandAttribute) == null;
            if (provideXP)
                await service.ModifyProfile(profile, x => x.XP += new Random().Next(0, profile.Level * data.XPMultiplier));

            var moneyCommandExecuted = ctx.Command.CustomAttributes.FirstOrDefault(x => x is MoneyCommandAttribute) != null;
            if (moneyCommandExecuted)
                await service.ModifyProfile(profile, x => x.CoinsBankMax += new Random().Next(0, profile.Level * data.CoinMultiplier));

            await base.AfterExecutionAsync(ctx);
        }

        [Command("profile")]
        [Description("Gets the profile of a user"), NonXPCommand]
        public async Task GetProfile(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username, true);

            if(profile == null)
            {
                var nullEmbed = new DiscordEmbedBuilder
                {
                    Title = ctx.Member.Username,
                    Description = "Does not have an account",
                    Color = ModuleInfo.Color
                };

                await ctx.Channel.SendMessageAsync(nullEmbed).ConfigureAwait(false);

                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = profile.Name,
                Color = DiscordColor.Gold
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)
             .AddField("ID: ", profile.Id.ToString())
             .AddField("Coins: ", $"{profile.Coins} {data.coinsEmoji}")
             .AddField("Bank: ", $"{profile.CoinsBank} {data.coinsEmoji} (max: {profile.CoinsBankMax})");

            embed.AddField("Level: ", $"{profile.Level}");
            embed.AddField("Job: ", profile.Job);

            string boosts = string.Empty;
            int index = 1;
            foreach (var boost in await service.GetBoosts(ctx.Member.Id))
            {
                boosts += $"{index}. {boost.BoosteName}\n";
                index++;
            }
            embed.AddField("Boosts:\n", boosts == string.Empty ? "None" : boosts);

            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        [Command("profile")]
        [Description("Gets the profile of a user"), NonXPCommand]
        public async Task GetProfile(CommandContext ctx, DiscordMember member = null)
        {
            var profile = await service.GetProfile(member.Id, member.Username, false);

            if (profile == null)
            {
                var nullEmbed = new DiscordEmbedBuilder
                {
                    Title = member.Username,
                    Description = "Does not have an account",
                    Color = ModuleInfo.Color
                };

                await ctx.Channel.SendMessageAsync(nullEmbed).ConfigureAwait(false);

                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = profile.Name,
                Color = DiscordColor.Gold
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)
             .AddField("ID: ", profile.Id.ToString())
             .AddField("Coins: ", $"{profile.Coins} {data.coinsEmoji}")
             .AddField("Bank: ", $"{profile.CoinsBank} {data.coinsEmoji} (max: {profile.CoinsBankMax})");

            embed.AddField("Level: ", $"{profile.Level}");
            embed.AddField("Job: ", profile.Job);

            string boosts = string.Empty;
            int index = 1;
            foreach (var boost in await service.GetBoosts(ctx.Member.Id))
            {
                boosts += $"{index}. {boost.BoosteName}\n";
                index++;
            }
            embed.AddField("Boosts:\n", boosts == string.Empty ? "None" : boosts);

            await ctx.RespondAsync(embed).ConfigureAwait(false);
        }

        [Command("Deposit")]
        [Aliases("dep")]
        [Description("Deposits Money into the bank"), RequireProfile]
        public async Task Deposit(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            if (profile.CoinsBank == profile.CoinsBankMax)
            {
                await ctx.RespondAsync($"Your your bank is full. Use a bank card to gain more space.");
                return;
            }
            if (profile.Coins <= 0)
            {
                await ctx.RespondAsync($"Your wallet is empty, can't deposit coins you don't have.");
                return;
            }

            var difference = profile.CoinsBankMax - profile.CoinsBank;
            int toDep;

            if (amount.ToLower().Equals("max"))
                toDep = System.Math.Min(difference, profile.Coins);
            else if (int.TryParse(amount, out int x))
            {
                toDep = int.Parse(amount);

                if (toDep > difference)
                {
                    await ctx.RespondAsync("You can't deposit more than your bank can hold?");
                    return;
                }
                if (toDep == 0)
                {
                    await ctx.RespondAsync("Dude, whats the point of depositing 0");
                    return;
                }
                if (toDep < 0)
                {
                    await ctx.RespondAsync("If you want to depsoit negative numbers just use the `withdraw` command");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync("You do know coins are measured using numbers right?");
                return;
            }

            await service.ModifyProfile(profile, x =>
            {
                x.CoinsBank += toDep;
                x.Coins -= toDep;
            });
            await ctx.RespondAsync($"Deposited {toDep} {data.coinsEmoji}");
            ExecutionRewards = true;
        }

        [Command("Withdraw")]
        [Aliases("with")]
        [Description("Withdraws Money into the bank"), RequireProfile]
        public async Task Withdraw(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            if(profile.CoinsBank <= 0)
            {
                await ctx.RespondAsync("Your bank is empty?");
                return;
            }

            int toWith;

            if (amount.ToLower().Equals("max"))
                toWith = profile.CoinsBank;
            else if (int.TryParse(amount, out int x))
            {
                toWith = int.Parse(amount);

                if (toWith > profile.CoinsBank)
                {
                    await ctx.RespondAsync("You can't withdraw more money than there is in your bank?");
                    return;
                }
                if(toWith == 0)
                {
                    await ctx.RespondAsync("Dude, whats the point of withdrawing 0");
                    return;
                }
                if (toWith < 0)
                {
                    await ctx.RespondAsync("If you want to withdraw negative numbers just use the `deposit` command");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync("You do know coins are measured using numbers right?");
                return;
            }

            await service.ModifyProfile(profile, x =>
            {
                x.CoinsBank -= toWith;
                x.Coins += toWith;
            });

            await ctx.RespondAsync($"Withdrawed {toWith} {data.coinsEmoji}");
            ExecutionRewards = true;
        }

        [Command("Lend")]
        [Description("Lend another user money"), RequireProfile, MoneyCommand]
        public async Task Lend(CommandContext ctx, DiscordMember member, string amount)
        {
            if (member.Id == ctx.Member.Id)
            {
                await ctx.RespondAsync("Whats the point of lending money to yourself?").ConfigureAwait(false);
                return;
            }

            var other = await service.GetProfile(member.Id, member.Username, false);
            if(other == null)
            {
                await ctx.RespondAsync($"{member.Mention} does not have a profile").ConfigureAwait(false);
                return;
            }

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            int toLend;

            if (amount.Equals("max"))
            {
                if (profile.Coins <= 0)
                {
                    await ctx.RespondAsync("You're broke dude...").ConfigureAwait(false);
                    return;
                }

                toLend = profile.Coins;
            }
            else if (int.TryParse(amount, out int x))
            {
                toLend = int.Parse(amount);

                if (profile.Coins <= 0)
                {
                    await ctx.RespondAsync("You're broke dude...").ConfigureAwait(false);
                    return;
                }
                if(toLend == 0)
                {
                    await ctx.RespondAsync("What exactly is the point of lending 0 coins?").ConfigureAwait(false);
                    return;
                }
                if (toLend == 0)
                {
                    await ctx.RespondAsync("If you want to lend them a negative amount of coins might as well ask them to lend you the same number of coins").ConfigureAwait(false);
                    return;
                }
                if (profile.Coins < toLend)
                {
                    await ctx.RespondAsync("You can't lend money you don't have").ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync("You do know coins are measured using numbers right?").ConfigureAwait(false);
                return;
            }

            await service.ModifyProfile(profile, x => x.Coins -= toLend);
            await service.ModifyProfile(other, x => x.Coins += toLend);

            await ctx.RespondAsync($"You lended {toLend} {data.coinsEmoji} to {member.Mention}").ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Daily")]
        [Description("Log in daily to collect some coins"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Daily(CommandContext ctx)
        {
            var coins = new Random().Next(data.dailyMin, data.dailyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Daily Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the day.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Weekly")]
        [Description("Log in weekly to collect some coins"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Weekly(CommandContext ctx)
        {
            var coins = new Random().Next(data.weeklyMin, data.weeklyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Weekly Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the week.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Monthly")]
        [Description("Log in monthly to collect some coins, for simplcity a month is averegaed to 30 days"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Monthly(CommandContext ctx)
        {
            var coins = new Random().Next(data.monthlyMin, data.monthlyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Monthly Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the monthly.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("SafeMode")]
        [Aliases("sm")]
        [Description("Toggles safe mode"), RequireProfile, NonXPCommand]
        public async Task SafeMode(CommandContext ctx)
        {
            await service.ModifyProfile(ctx.Member.Id, x => x.SafeMode = x.SafeMode == 1 ? 0 : 1).ConfigureAwait(false);
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            await ctx.RespondAsync($"Safe mode set to {profile.SafeMode == 1}").ConfigureAwait(false);
        }

        [Command("JobList")]
        [Description("can't remain unemployed can you?")]
        public async Task JobList(CommandContext ctx)
        {
            var jobs = Job.AllJobs;
            int level = (await service.GetProfile(ctx.Member.Id, ctx.Member.Username)).Level;

            var embeds = new List<DiscordEmbedBuilder>();
            DiscordEmbedBuilder current = null;

            int index = 0, maxPerPage = 7;
            foreach (var job in jobs)
            {
                if (index % maxPerPage == 0)
                {
                    current = new DiscordEmbedBuilder().WithTitle($"Job List")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Use the \"currency hire\" command to get a job, Message will remain active for 1 minute", ctx.User.AvatarUrl);

                    embeds.Add(current);
                }

                current.AddField($"{++index}. {(level < job.minLvlNeeded ? data.cross : data.tick)} **{job.Name}** {job.emoji}", $"`Min Level:` { job.minLvlNeeded}\n `Avg wage:` { (job.SucceedMax + job.SucceedMin) / 2}");
            }

            var pages = embeds.Select(x => new Page(null, x));
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.DeleteButtons, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Apply")]
        [Description("Apply for a job")]
        [RequireProfile, RequireJob(false), CheckWorkDate]
        public async Task Hire(CommandContext ctx, string jobName)
        {
            var job = Job.AllJobs.FirstOrDefault(x => x.Name.ToLower() == jobName.ToLower());
            if (job == null)
                await ctx.RespondAsync("The given job was not found");

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            int level = (await service.GetProfile(ctx.Member.Id, ctx.Member.Username)).Level;

            if (level < job.minLvlNeeded)
            {
                await ctx.RespondAsync($"The min level needed for the job is {job.minLvlNeeded}, your level is {level}");
                return;
            }

            await service.ModifyProfile(profile, x => x.Job = job.Name);

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Cogratulations",
                Description = $"{ctx.Member.Mention} you have been hired. \nUse the work command to earn some money.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Resign")]
        [Description("Resign from a job")]
        [RequireProfile, RequireJob]
        public async Task Resign(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var prevJob = profile.Job;

            await service.ModifyProfile(profile, x => x.Job = "None");

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Resigned",
                Description = $"{ctx.Member.Mention} has resigned from being a {prevJob}",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);

            await service.ModifyProfile(profile, x => x.PrevWorkDate = DateTime.Now.ToString());
            ExecutionRewards = true;
        }

        Func<int, int, int> GenerateRandom = (int min, int max) => new Random().Next(min, max);

        [Command("Work")]
        [Description("Want to make some money?")]
        [RequireProfile, RequireJob, CheckWorkDate]
        public async Task Work(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.User.AvatarUrl,
                Height = data.thumbnailSize,
                Width = data.thumbnailSize
            };
            var steps = await job.GetWork(ModuleInfo.Color, thumbnail);     

            DialogueHandlerConfig dialogueConfig = new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = true
            };

            DialogueHandler handler = new DialogueHandler(dialogueConfig, steps); 
            var completed = await handler.ProcessDialogue();

            int money = completed?  GenerateRandom(job.SucceedMin, job.SucceedMax) : GenerateRandom(job.FailMin, job.FailMax);

            var completedEmbed = new DiscordEmbedBuilder
            {
                Title = completed ? "Good Job" : "Time Out",
                Description = completed ? $"You recieve {money} coins for a job well done" : $"You recieve only {money} coins",
                Color = ModuleInfo.Color,
                Thumbnail = thumbnail
            };

            await service.ModifyProfile(profile, x =>
            {
                x.Coins += money;
                x.PrevWorkDate = DateTime.Now.ToString();
            });

            await ctx.RespondAsync(completedEmbed).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Shop")]
        [Description("Diaplays all items in the shop"), NonXPCommand]
        public async Task ShowShop(CommandContext ctx)
        {
            var items = Shop.AllItems;
            var embeds = new List<DiscordEmbedBuilder>();
            DiscordEmbedBuilder current = null;

            int index = 0, maxPerPage = 7;
            foreach (var item in items)
            {
                if (index % maxPerPage == 0)
                {
                    current = new DiscordEmbedBuilder().WithTitle($"Shop")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Message will remain active for 1 minute", ctx.User.AvatarUrl);

                    embeds.Add(current);
                }

                current.AddField($"{++index}. {item.Name}\n", $" Description: {item.Description}\nPrice: {item.Price}\n\n");
            }

            var pages = embeds.Select(x => new Page(null, x));
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.DeleteButtons, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Item")]
        [Description("Gives info about an item"), NonXPCommand]
        public async Task Item(CommandContext ctx, [RemainingText] string itemName)
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
                Width = data.thumbnailSize,
                Height = data.thumbnailSize,
                Url = emoji.Url
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = item.Name,
                Description = item.Description,
                Thumbnail = thumbnail,
                Color = ModuleInfo.Color
            };

            embed.AddField("Price: ", item.Price.ToString(), true);
            embed.AddField("Owned: ", proilfeItem.Count.ToString(), true);

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Buy")]
        [Description("Buy an item"), RequireProfile, MoneyCommand]
        public async Task BuyItem(CommandContext ctx, int quantity, [RemainingText]string itemName)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var result = Shop.Buy(itemName, quantity, in profile);
            if (!result.completed)
                await ctx.RespondAsync(result.message).ConfigureAwait(false);
            else
            {
                await service.ModifyProfile(profile, x => x.Coins -= result.item.Price * quantity);
                await service.AddOrRemoveItem(ctx.Member.Id, result.item.Name, quantity);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Purchase Successful",
                    Description = result.message,
                    Color = ModuleInfo.Color
                }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);
                ExecutionRewards = true;
            }
        }

        [Command("Sell")]
        [Description("Sell an item")]
        [RequireProfile, MoneyCommand]
        public async Task Sell(CommandContext ctx, int quantity, [RemainingText] string itemName)
        {
            var result = Shop.GetItem(itemName);
            if (result == null)
            {
                await ctx.RespondAsync("This item doesn't exist?");
                return;
            }

            var item = service.GetItem(ctx.Member.Id, result.Name);           
            if (item == null)
                await ctx.RespondAsync("You don't have this item?").ConfigureAwait(false);
            else
            {
                await service.ModifyProfile(ctx.Member.Id, x => x.Coins += result.SellingPrice * quantity);
                await service.AddOrRemoveItem(ctx.Member.Id, result.Name, -quantity);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Sold item",
                    Description = $"Successfuly sold {quantity} {result.Name}(s) for {quantity * result.SellingPrice}",
                    Color = ModuleInfo.Color
                }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);
                ExecutionRewards = true;
            }
        }

        [Command("Gift")]
        [Description("Gift items to other members")]
        [RequireProfile, MoneyCommand]
        public async Task Gift(CommandContext ctx, DiscordMember member, int number, [RemainingText]string itemName)
        {
            var other = service.GetProfile(member.Id, member.Username, false);
            if (other == null)
            {
                await ctx.RespondAsync($"{member.Mention} does not have a profile").ConfigureAwait(false);
                return;
            }

            var result = Shop.GetItem(itemName);
            if (result == null)
            {
                await ctx.RespondAsync("The item doesn't exist??");
                return;
            }

            var item = await service.GetItem(ctx.Member.Id, result.Name);
            if (item == null)
                await ctx.RespondAsync("You don't have this item??").ConfigureAwait(false);
            else if (item.Count < number)
                await ctx.RespondAsync($"You only have {item.Count} {item.Name}(s)");
            else
            {
                await service.AddOrRemoveItem(ctx.Member.Id, result.Name, -number);
                await service.AddOrRemoveItem(member.Id, result.Name, number);

                await ctx.RespondAsync($"{ctx.Member.Mention} gave {number} {result.Name}(s) to {member.Mention}").ConfigureAwait(false);
                ExecutionRewards = true;
            }
        }

        [Command("Inventory")]
        [Description("Displays an users inventory")]
        [RequireProfile, Aliases("Inven"), NonXPCommand]
        public async Task ShowInventory(CommandContext ctx)
        {
            var items = await service.GetItems(ctx.Member.Id);
            var embeds = new List<DiscordEmbedBuilder>();
            DiscordEmbedBuilder current = null;

            int index = 0, maxPerPage = 7;
            foreach (var item in items)
            {
                if (index % maxPerPage == 0)
                {
                    current = new DiscordEmbedBuilder().WithTitle($"{ctx.Member.Username}'s Inventory")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Message will remain active for 1 minute")
                        .WithAuthor(ctx.User.Username, ctx.User.AvatarUrl, ctx.User.AvatarUrl);

                    embeds.Add(current);
                }

                current.AddField($"{++index}. {item.Name}\n", $" Count: {item.Count}\n\n");
            }

            if (embeds.Count == 0)
                embeds.Add(new DiscordEmbedBuilder().WithTitle($"{ctx.Member.Username}'s Inventory")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Message will remain active for 1 minute", ctx.User.AvatarUrl)
                        .WithDescription("Inventory is empty"));

            var pages = embeds.Select(x => new Page(null, x));
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.DeleteButtons, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Inventory"), NonXPCommand]
        public async Task ShowOtherInventory(CommandContext ctx, DiscordMember member = null)
        {
            var profile = await service.GetProfile(member.Id, member.Username, false);

            if (profile == null)
            {
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = member.Username,
                    Description = "Does not have an account",
                    Color = ModuleInfo.Color
                }).ConfigureAwait(false);

                return;
            }

            var items = await service.GetItems(member.Id);
            var embeds = new List<DiscordEmbedBuilder>();
            DiscordEmbedBuilder current = null;

            int index = 0, maxPerPage = 7;
            foreach (var item in items)
            {
                if (index % maxPerPage == 0)
                {
                    current = new DiscordEmbedBuilder().WithTitle($"{ctx.Member.Username}'s Inventory")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Message will remain active for 1 minute", ctx.User.AvatarUrl);

                    embeds.Add(current);
                }

                current.AddField($"{++index}. {item.Name}\n", $" Count: {item.Count}\n\n");
            }

            if (embeds.Count == 0)
                embeds.Add(new DiscordEmbedBuilder().WithTitle($"{ctx.Member.Username}'s Inventory")
                        .WithColor(ModuleInfo.Color)
                        .WithFooter("Message will remain active for 1 minute", ctx.User.AvatarUrl)
                        .WithDescription("Inventory is empty"));

            var pages = embeds.Select(x => new Page(null, x));
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.DeleteButtons, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Use")]
        [Description("Use an item"), RequireProfile]
        public async Task Use(CommandContext ctx, [RemainingText] string itemName)
        {
            var item = Shop.GetItem(itemName);
            if(item == null)
            {
                await ctx.RespondAsync("This item doesn't exist??").ConfigureAwait(false);
                return;
            }

            var itemData = await service.GetItem(ctx.Member.Id, item.Name);
            if (itemData == null)
            {
                await ctx.RespondAsync("You don't have this item??").ConfigureAwait(false);
                return;
            }

            var useResult = await item.Use(service, ctx.Member);
            if (!useResult.useComplete)
            {
                await ctx.RespondAsync(useResult.message).ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Item Successfully Used",
                Description = useResult.message,
            }.WithFooter($"Item: {itemData.Name}").WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Meme")]
        [Description("The currency meme command")]
        [RequireProfile, PresenceItem(PresenceData.PresenceCommand.Meme), MoneyCommand]
        public async Task Meme(CommandContext ctx)
        {
            var itemNeed = Shop.GetPresneceItem(ctx);

            var itemData = await service.GetItem(ctx.Member.Id, itemNeed.Name).ConfigureAwait(false);
            if(itemData == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = $"You need a {itemNeed.Name} to run this command, you have 0",
                    Color = ModuleInfo.Color
                }.WithFooter($"User: {ctx.Member.Username}")).ConfigureAwait(false);

                return;
            }

            var casted = itemNeed as PresenceItem;
            var reward = casted.Data.GetReward();

            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += reward);

            string descripion = reward switch
            {
                0 => "Well no one liked your meme lmao, you get 0 coins",
                var y when y < casted.Data.maxReward / 2 => $"Your meme got a half decent reponse online, good job. You get {reward} coins",
                _ => $"Damn your meme is **Trending**, got to say im impressed. Here you go, {reward} coins"
            };

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Memes",
                Description = descripion,
                Color = ModuleInfo.Color
            }.WithThumbnail(ctx.User.AvatarUrl, data.thumbnailSize, data.thumbnailSize)
             .WithFooter($"User: {ctx.Member.Username}")).ConfigureAwait(false);
        }
    }
}
