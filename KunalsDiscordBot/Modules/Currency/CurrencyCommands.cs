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

using KunalsDiscordBot.Core;
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.DialogueHandlers;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.DialogueHandlers.Steps;
using KunalsDiscordBot.Core.Modules.CurrencyCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.CurrencyModels;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Boosts.Interfaces;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shop;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    [ModuleLifespan(ModuleLifespan.Transient), Aliases("crncy", "c")]
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
            data = configurationManager.CurrenyConfig;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Currency];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var customAttributes = ctx.Command.CustomAttributes;
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username, false);

            var requireProfile = customAttributes.FirstOrDefault(x => x is RequireProfileAttribute) != null;
            if(requireProfile && profile == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "You need a profile to run this command",
                    Color = ModuleInfo.Color
                }.WithFooter("Use the `currency profile` command to create a profile")).ConfigureAwait(false);

                throw new CustomCommandException();
            }

            var jobCheck = customAttributes.FirstOrDefault(x => x is RequireJobAttribute);
            if(jobCheck != null)
            {
                if (profile.Job.Equals("None") && ((RequireJobAttribute)jobCheck).require)
                {
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Description = "You need a job to run this command",
                        Color = ModuleInfo.Color
                    }.WithFooter("Use the \"currency apply\" command to get a job")).ConfigureAwait(false);

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

            var checkWorkDate = customAttributes.FirstOrDefault(x => x is CheckWorkDateAttribute) != null;
            if(checkWorkDate)
            {
                var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);

                if (DateTime.TryParse(profile.PrevWorkDate, out var x))
                {
                    var prevDate = DateTime.Parse(profile.PrevWorkDate);

                    var timeSpan =  DateTime.Now - prevDate;
                    if (timeSpan.TotalHours < (profile.Job == "None" ? Job.resignTimeSpan.TotalHours : job.CoolDown))
                    {
                        var timeString = $"{timeSpan.Days} days, {timeSpan.Hours} hours, {timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds";

                        await ctx.RespondAsync(new DiscordEmbedBuilder
                        {
                            Title = "Chill out",
                            Description = profile.Job == "None" ? $"You just resigned and its only been {timeString}, have to wait {Job.resignTimeSpan} hours before you can apply again" : $"Its only been {timeString} since your last shift, {job.CoolDown} hours is the minimum time.",
                            Color = ModuleInfo.Color
                        }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);

                        throw new CustomCommandException();
                    }
                }
            }

            var presenceItemNeeded = customAttributes.FirstOrDefault(x => x is PresenceItemAttribute);
            if(presenceItemNeeded != null)
            {
                var itemNeeded = Shop.GetPresneceItem(ctx.Command);

                var itemData = await service.GetItem(ctx.Member.Id, itemNeeded.Name).ConfigureAwait(false);
                if (itemData == null)
                {
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Description = $"You need a {itemNeeded.Name} to run this command, you have 0",
                        Color = ModuleInfo.Color
                    }.WithFooter($"User: {ctx.Member.Username}")).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            var noSafeModeNeeded = customAttributes.FirstOrDefault(x => x is NoSafeModeCommandAttribute) != null;
            if(noSafeModeNeeded && profile.SafeMode == 1)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "You have safe mode enabled and can't run this command",
                    Color = ModuleInfo.Color
                }.WithFooter("Use the `currency safemode` to toggle safe mode")).ConfigureAwait(false);

                throw new CustomCommandException();
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
            {
                await service.ModifyProfile(profile, x =>
                {
                    x.XP += new Random().Next(0, profile.Level * data.XPMultiplier);
                    x.Level = (int)(data.LevelConstant * System.Math.Sqrt(x.XP)) + 1;
                });
            }

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
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)
             .AddField("ID: ", profile.Id.ToString())
             .AddField("Coins: ", $"{profile.Coins} {data.CoinsEmoji}")
             .AddField("Bank: ", $"{profile.CoinsBank} {data.CoinsEmoji} (max: {profile.CoinsBankMax})")
             .AddField("Level: ", $"{profile.Level}")
             .AddField("XP: ", $"{profile.XP}", true)
             .AddField("Job: ", profile.Job);

            var boosts = await service.GetBoosts(ctx.Member.Id);
            embed.AddField("Boosts:\n", !boosts.Any() ? "None" : string.Join(", ", boosts.Select(x => $"`{x.Name}`")));

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
            }.WithThumbnail(member.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)
             .AddField("ID: ", profile.Id.ToString())
             .AddField("Coins: ", $"{profile.Coins} {data.CoinsEmoji}")
             .AddField("Bank: ", $"{profile.CoinsBank} {data.CoinsEmoji} (max: {profile.CoinsBankMax})")
             .AddField("Level: ", $"{profile.Level}")
             .AddField("XP: ", $"{profile.XP}", true)
             .AddField("Job: ", profile.Job);

            var boosts = await service.GetBoosts(ctx.Member.Id);
            embed.AddField("Boosts:\n", !boosts.Any() ? "None" : string.Join(", ", boosts.Select(x => $"`{x.Name}`")));

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
            await ctx.RespondAsync($"Deposited {toDep} {data.CoinsEmoji}");
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

            await ctx.RespondAsync($"Withdrawed {toWith} {data.CoinsEmoji}");
            ExecutionRewards = true;
        }

        [Command("Give")]
        [Description("Give another user money free of interest"), RequireProfile, MoneyCommand]
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

            await ctx.RespondAsync($"You lended {toLend} {data.CoinsEmoji} to {member.Mention}").ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Daily")]
        [Description("Log in daily to collect some coins"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Daily(CommandContext ctx)
        {
            var coins = new Random().Next(data.DailyMin, data.DailyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Daily Coins",
                Description = $"{coins} {data.CoinsEmoji}, Here are your coins for the day.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Weekly")]
        [Description("Log in weekly to collect some coins"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Weekly(CommandContext ctx)
        {
            var coins = new Random().Next(data.WeeklyMin, data.WeeklyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Weekly Coins",
                Description = $"{coins} {data.CoinsEmoji}, Here are your coins for the week.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Monthly")]
        [Description("Log in monthly to collect some coins, for simplcity a month is averegaed to 30 days"), MoneyCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Monthly(CommandContext ctx)
        {
            var coins = new Random().Next(data.MonthlyMin, data.MonthlyMax);
            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += coins).ConfigureAwait(false); 

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Monthly Coins",
                Description = $"{coins} {data.CoinsEmoji}, Here are your coins for the monthly.",
                Color = ModuleInfo.Color,
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
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

            var pages = ctx.Client.GetInteractivity().GetPages(jobs.ToList(), x => ($"{(level < x.minLvlNeeded ? data.Cross : data.Tick)} **{x.Name}** {x.emoji}",
            $"`Min Level:` { x.minLvlNeeded}\n `Avg wage:` { (x.SucceedMax + x.SucceedMin) / 2}"), new EmbedSkeleton
            {
                Title = "Joblist",
                Color = ModuleInfo.Color,
                Footer = new DiscordEmbedBuilder.EmbedFooter { IconUrl = ctx.User.AvatarUrl, Text = "Use the \"currency apply\" command to get a job, Message will remain active for 1 minute" }
            }, 7, false).ToList(); ;

            if (pages.Count == 1)
                await ctx.Channel.SendMessageAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Apply")]
        [Description("Apply for a job")]
        [RequireProfile, RequireJob(false), CheckWorkDate]
        public async Task Apply(CommandContext ctx, string jobName)
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
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
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
            }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);

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
                Height = data.ThumbnailSize,
                Width = data.ThumbnailSize
            };

            var steps = await job.GetWork();

            var messageData = new MessageData { Reply = true, ReplyId = ctx.Message.Id , Thumbnail = thumbnail , Color = ModuleInfo.Color};
            steps.ForEach(x => x.WithMesssageData(messageData));

            DialogueHandler handler = new DialogueHandler(new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = true,
                RequireFullCompletion = true,
                QuickStart = false
            }).WithSteps(steps);

            var result = await handler.ProcessDialogue();
            int money = result == null ? GenerateRandom(job.FailMin, job.FailMax) : GenerateRandom(job.SucceedMin, job.SucceedMax);

            await service.ModifyProfile(profile, x =>
            {
                x.Coins += money;
                x.PrevWorkDate = DateTime.Now.ToString();
            });

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = result == null ? "Time Out" : "Good Job" ,
                Description = result == null ? $"You recieve only {money} coins" : $"You recieve {money} coins for a job well done",
                Color = ModuleInfo.Color,
                Thumbnail = thumbnail
            }).ConfigureAwait(false);

            ExecutionRewards = true;
        }

        [Command("Shop")]
        [Description("Diaplays all items in the shop"), NonXPCommand]
        public async Task ShowShop(CommandContext ctx)
        {
            var items = Shop.AllItems;

            var pages = ctx.Client.GetInteractivity().GetPages(items, x => ($"{x.Name}", $"Description: {x.Description}\nPrice: {x.Price}"), new EmbedSkeleton
            {
                Title = "Shop",
                Description = $"Stock is {(StockMarket.Instance.CurrentStockPrice > 0 ? "up" : "down")} by **{System.Math.Abs(StockMarket.Instance.CurrentStockPrice)}%**",
                Color = ModuleInfo.Color,
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName }
            }, 7, false).ToList(); ;

            if (pages.Count == 1)
                await ctx.Channel.SendMessageAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
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

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = item.Name,
                Description = item.Description,
                Color = ModuleInfo.Color
            }.AddField("Price: ", item.Price.ToString(), true)
            .AddField("Owned: ", proilfeItem.Count.ToString(), true)).ConfigureAwait(false);
        }

        [Command("Buy")]
        [Description("Buy an item"), RequireProfile, MoneyCommand]
        public async Task Buy(CommandContext ctx, int quantity, [RemainingText]string itemName)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var result = Shop.Buy(itemName, quantity, in profile);
            if (!result.completed)
                await ctx.RespondAsync(result.message).ConfigureAwait(false);
            else
            {
                await service.ModifyProfile(profile, x => x.Coins -= result.item.Price * quantity);
                await service.AddOrRemoveItem(profile, result.item.Name, quantity);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Purchase Successful",
                    Description = result.message,
                    Color = ModuleInfo.Color
                }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
                ExecutionRewards = true;
            }
        }

        [Command("Buy")]
        [RequireProfile, MoneyCommand]
        public async Task BuySingle(CommandContext ctx, [RemainingText] string itemName)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            var result = Shop.Buy(itemName, 1, in profile);
            if (!result.completed)
                await ctx.RespondAsync(result.message).ConfigureAwait(false);
            else
            {
                await service.ModifyProfile(profile, x => x.Coins -= result.item.Price);
                await service.AddOrRemoveItem(profile, result.item.Name, 1);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Purchase Successful",
                    Description = result.message,
                    Color = ModuleInfo.Color
                }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
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

            var item = await service.GetItem(ctx.Member.Id, result.Name);
            if (item == null)
                await ctx.RespondAsync("You don't have this item?").ConfigureAwait(false);
            else if (item.Count < quantity)
                await ctx.RespondAsync($"You only have {item.Count} {result.Name}(s)");
            else
            {
                await service.ModifyProfile(ctx.Member.Id, x => x.Coins += result.SellingPrice * quantity);
                await service.AddOrRemoveItem(ctx.Member.Id, result.Name, -quantity);

                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Sold item",
                    Description = $"Successfuly sold {quantity} {result.Name}(s) for {quantity * result.SellingPrice}",
                    Color = ModuleInfo.Color
                }.WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
                ExecutionRewards = true;
            }
        }

        [Command("Gift")]
        [Description("Gift items to other members")]
        [RequireProfile, MoneyCommand]
        public async Task Gift(CommandContext ctx, DiscordMember member, int number, [RemainingText]string itemName)
        {
            var other = await service.GetProfile(member.Id, member.Username, false);
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
                await service.AddOrRemoveItem(other, result.Name, number);

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

            var pages = ctx.Client.GetInteractivity().GetPages(items, x => ($"{x.Name}", $" Count: {x.Count}"), new EmbedSkeleton
            {
                Title = $"{ctx.Member.Username}'s Inventory",
                Color = ModuleInfo.Color,
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName }
            }, 7, false).ToList();

            if (pages == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithTitle($"{ctx.Member.Username}'s Inventory")
                    .WithDescription("Inventory is empty")
                    .WithColor(ModuleInfo.Color));
            else if(pages.Count == 1)
                await ctx.Channel.SendMessageAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
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
            var pages = ctx.Client.GetInteractivity().GetPages(items, x => ($"{x.Name}", $" Count: {x.Count}"), new EmbedSkeleton
            {
                Title = $"{member.Username}'s Inventory",
                Color = ModuleInfo.Color,
                Author = new DiscordEmbedBuilder.EmbedAuthor { IconUrl = ctx.Member.AvatarUrl, Name = ctx.Member.DisplayName }
            }, 7, false).ToList(); 

            if (pages == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithTitle($"{member.Username}'s Inventory")
                    .WithDescription("Inventory is empty")
                    .WithColor(ModuleInfo.Color));
            else if (pages.Count == 1)
                await ctx.Channel.SendMessageAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
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

            var useResult = await item.Use(await service.GetProfile(ctx.Member.Id, ""), service);
            if (!useResult.UseComplete)
            {
                await ctx.RespondAsync(useResult.Message).ConfigureAwait(false);
                return;
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "Item Successfully Used",
                Description = useResult.Message,
                Color = ModuleInfo.Color
            }.WithFooter($"Item: {itemData.Name}").WithThumbnail(ctx.User.AvatarUrl, data.ThumbnailSize, data.ThumbnailSize)).ConfigureAwait(false);
            ExecutionRewards = true;
        }

        [Command("Meme")]
        [Description("Upload memes, earn coins")]
        [RequireProfile, PresenceItem(PresenceData.PresenceCommand.Meme), MoneyCommand]
        public async Task Meme(CommandContext ctx)
        {
            var itemNeed = Shop.GetPresneceItem(ctx.Command);

            var casted = itemNeed as PresenceItem;
            var reward = casted.Data.GetReward();

            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += reward);

            var options = new DiscordSelectComponent("meme", "Choose an option", data.Memes.Select(x => new DiscordSelectComponentOption(x, x)));
            var result = await new DropDownStep("What type of meme would you like to post?", "", 10, options).WithMesssageData(new MessageData { Reply = true, ReplyId = ctx.Message.Id })
                .ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false); 

            string descripion = reward switch
            {
                0 => "Well no one liked your meme lmao, you get 0 coins",
                var y when y < casted.Data.maxReward / 2 => $"Your meme got a half decent reponse online, good job. You get {reward} coins",
                _ => $"Damn your meme is **Trending**, got to say im impressed. Here you go, {reward} coins"
            };

            await result.Message.ModifyAsync(new DiscordMessageBuilder()
                    .WithContent(descripion)
                    .AddComponents(new DiscordSelectComponent(options.CustomId, options.Placeholder, options.Options, true)));

            await result.Message.ModifyAsync(descripion).ConfigureAwait(false);

            ExecutionRewards = true;
        }

        [Command("Sleep")]
        [Description("Sleep do be good")]
        [RequireProfile, PresenceItem(PresenceData.PresenceCommand.Sleep), MoneyCommand]
        public async Task Sleep(CommandContext ctx)
        {
            var itemNeed = Shop.GetPresneceItem(ctx.Command);

            var casted = itemNeed as PresenceItem;
            var reward = casted.Data.GetReward();

            await service.ModifyProfile(ctx.Member.Id, x => x.Coins += reward);

            string descripion = reward switch
            {
                0 => "Yea, you didn't have a good night sleep so you get no coins",
                var y when y < casted.Data.maxReward / 2 => $"Well, you had a half decent night. You get {reward} coins",
                _ => $"That might have been the best sleep you've ever had. Here you go sleeping beauty, {reward} coins"
            };

            await ctx.RespondAsync(descripion).ConfigureAwait(false);

            ExecutionRewards = true;
        }

        [Command("Hug")]
        [Description("Everybody loves a hug now don't they?")]
        [RequireProfile, PresenceItem(PresenceData.PresenceCommand.Sleep)]
        public async Task Hug(CommandContext ctx)
        {
            await ctx.RespondAsync("As you hug the waifu pillow, you feel a large amount of warmth").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync("https://tenor.com/view/hugs-sending-virtual-hugs-loading-gif-8158818").ConfigureAwait(false);
        }

        [Command("Steal")]
        [Aliases("Rob"), Description("Rob from another user"), NoSafeModeCommand]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Minute * 2, CooldownBucketType.User), MoneyCommand]
        public async Task Steal(CommandContext ctx, DiscordUser victim)
        {
            if (victim.Id == ctx.User.Id)
            {
                await ctx.RespondAsync("You can't rob yourself?");
                return;
            }
            var victimProfile = await service.GetProfile(victim.Id, "");

            if(victimProfile == null)
            {
                await ctx.RespondAsync($"{victim.Mention} doesn't have a profile");
                return;
            }
            if(victimProfile.SafeMode == 1)
            {
                await ctx.RespondAsync($"{victim.Mention} has safe mode enabled");
                return;
            }
            if (victimProfile.Coins <= 100)
            {
                await ctx.RespondAsync($"{victim.Mention} is almost broke lol, leave em alone");
                return;
            }

            var robberProfile = await service.GetProfile(ctx.User.Id, "");

            var boosts = (await service.GetBoosts(victim.Id).ConfigureAwait(false));
            var theftProtectionBoosts = boosts.Where(x => x is ITheftProtection).Cast<ITheftProtection>().OrderBy(x => x.Order).ToList();
            UseResult result = default;

            foreach (var boost in theftProtectionBoosts)
            {
                result = boost.MethodId switch
                {
                    1 => await boost.Use(victimProfile, service),
                    2 => await boost.Use(victimProfile, robberProfile, service),
                    _ => default
                };

                if (result.UseComplete)
                    break;
            }

            if (result.UseComplete)
                await ctx.RespondAsync(result.Message).ConfigureAwait(false);
            else
            {
                var luckBoost = boosts.FirstOrDefault(x => x is LuckBoost);
                var luck = luckBoost == null ? 0 : luckBoost.PercentageIncrease;

                var random = new Random();
                int coins; string message;

                switch(random.Next(1, 100) + luck)
                {
                    case var x when x < 10:
                        coins = 0;
                        message = "Yeah you didn't steal anything lmfao";
                        break;
                    case var x when x < 100:
                        coins = random.Next(0, (int)(x / 100f * victimProfile.Coins));
                        message = coins < victimProfile.Coins / 2 ? "You stole a decent amount coins" : "Holy crap you stole more coins than I expected";
                        break;
                    default:
                        coins = victimProfile.Coins;
                        message = "YOU STOLE BASICALLY EVERYTHING HAHA";
                        break;
                }

                await service.ModifyProfile(robberProfile, x => x.Coins += coins);
                await service.ModifyProfile(victimProfile, x => x.Coins -= coins);

                await ctx.RespondAsync($"{message}, here you go {coins} {data.CoinsEmoji}");
            }

            ExecutionRewards = true;
        }

        [Command("Search")]
        [Description("Search for coins in locations with a small chance to die")]
        [RequireProfile, Cooldown(1, 20, CooldownBucketType.User), Aliases("Look"), MoneyCommand]
        public async Task Search(CommandContext ctx)
        {
            var first3 = CurrencyModel.Locations.Select(x => x).ToList().Shuffle().Take(3);
            var buttonStep = new ButtonStep("Where would you like to search?", "", 10, first3.Select(x => new DiscordButtonComponent(ButtonStyle.Primary, x.Name, x.Name)).ToList())
                .WithMesssageData(new MessageData { Reply = true, ReplyId = ctx.Message.Id });

            var result = await buttonStep.ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);
            if(!result.WasCompleted)
            {
                await result.Message.ModifyAsync("Well you didn't reply so ¯\\_(ツ)_/¯");
                return;
            }
            var rng = new Random();

            var chosen = first3.First(x => result.Result == x.Name);

            var luckBoost = (await service.GetBoosts(ctx.Member.Id)).FirstOrDefault(x => x is LuckBoost);
            var luck = luckBoost == null ? 0 : luckBoost.PercentageIncrease;
            var die = (rng.Next(0, 100) + luck) > 85;

            var profile = await service.GetProfile(ctx.Member.Id, "");

            if(die)
            {
                await service.KillProfile(profile);

                await result.Message.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"**{ctx.User.Username} searched the {chosen.Name}**\n{chosen.FailureMessage}")
                    .WithColor(ModuleInfo.Color)));
            }
            else
            {
                var coins = rng.Next(chosen.MimimumCoins, chosen.MaximumCoins);
                await service.ModifyProfile(profile, x => x.Coins += coins);

                await result.Message.ModifyAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    .WithDescription($"**{ctx.User.Username} searched the {chosen.Name}**\n{chosen.SuccedMessage.Replace("{}", coins.ToString())}")
                    .WithColor(ModuleInfo.Color)));
            }

            ExecutionRewards = true;
        }

        [Command("SimpleBet")]
        [Description("Roll a a random number from 1 - 20 against the bot and bet a sum of money. If you win you get as much money as you bet without losing any, if you lose you lose all the money you bet")]
        [RequireProfile, Cooldown(1, 20, CooldownBucketType.User), MoneyCommand, Aliases("Bet")]
        public async Task SimpleBet(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, "");
            if (profile.Coins < 200)
            {
                await ctx.RespondAsync("You need at least 200 coins to bet");
                return;
            }

            int coins = 0;
            if (amount.ToLowerInvariant().Equals("max"))
                coins = profile.Coins;
            else if (int.TryParse(amount, out var x))
            {
                coins = int.Parse(amount);

                if (coins <= 0)
                {
                    await ctx.RespondAsync("Enter a positive number to bet");
                    return;
                }
                else if(coins > profile.Coins)
                {
                    await ctx.RespondAsync("You can't bet more coins than you have?");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync("You do know coins are measured in numbers right?");
                return;
            }

            var rng = new Random();
            int playerRoll = rng.Next(1, 21), botRoll = rng.Next(1, 21), reward =  playerRoll == botRoll ? 0 : (playerRoll > botRoll ? coins : - coins );

            if (reward != 0)
                await service.ModifyProfile(profile, x => x.Coins += reward);

            await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithColor(ModuleInfo.Color)
                    .WithTitle($"{ctx.Member.Username}'s bet")
                    .AddField($"{ctx.Member.DisplayName} rolls", playerRoll.ToString(), true)
                    .AddField("I roll", botRoll.ToString(), true)
                    .AddField("Result", $"{(reward == 0 ? "Is a draw, you neither lose nor gain money" : (reward > 0 ? $"You get {coins} {data.CoinsEmoji}" : $"You lose {coins} {data.CoinsEmoji}"))}"));

            ExecutionRewards = true;
        }

        [Command("ExtremeBet")]
        [Description("Roll a a random number from 1 - 20 against the bot and bet a sum of money. If you win you get double the money, if you lose you die regardless of any boosts")]
        [RequireProfile, Cooldown(1, 20, CooldownBucketType.User), MoneyCommand]
        public async Task ExtremeBet(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, "");
            if(profile.Coins < 200)
            {
                await ctx.RespondAsync("You need at least 200 coins to bet");
                return;
            }

            int coins = 0;
            if (amount.ToLowerInvariant().Equals("max"))
                coins = profile.Coins;
            else if (int.TryParse(amount, out var x))
            {
                coins = int.Parse(amount);

                if (coins <= 0)
                {
                    await ctx.RespondAsync("Enter a positive number to bet");
                    return;
                }
                else if (coins > profile.Coins)
                {
                    await ctx.RespondAsync("You can't bet more coins than you have?");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync("You do know coins are measured in numbers right?");
                return;
            }

            var rng = new Random();
            int playerRoll = rng.Next(1, 21), botRoll = rng.Next(1, 21), reward = playerRoll == botRoll ? 0 : (playerRoll > botRoll ? coins * 2 : -1);

            if (reward > 0)
                await service.ModifyProfile(profile, x => x.Coins += reward);
            else if (reward < 0)
                await service.KillProfile(profile, false);

            await ctx.RespondAsync(new DiscordEmbedBuilder()
                    .WithColor(ModuleInfo.Color)
                    .WithTitle($"{ctx.Member.Username}'s bet")
                    .AddField($"{ctx.Member.DisplayName} rolls", playerRoll.ToString(), true)
                    .AddField("I roll", botRoll.ToString(), true)
                    .AddField("Result", $"{(reward == 0 ? "Is a draw, you neither lose nor gain money" : (reward > 0 ? $"You get {reward} {data.CoinsEmoji}" : "Welp time to die"))}"));

            ExecutionRewards = true;
        }
    }
}
