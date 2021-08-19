using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.DialogueHandlers;
using KunalsDiscordBot.Core.Modules.CurrencyCommands;
using KunalsDiscordBot.Core.Attributes.CurrencyCommands;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Items;

namespace KunalsDiscordBot.Modules.Currency
{
    [Group("Currency")]
    [Decor("Gold", ":coin:")]
    [Description("A currency system!")]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AccessChannels))]
    public sealed partial class CurrencyCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IProfileService service;
        private readonly CurrencyData data;

        public CurrencyCommands(IProfileService _service, PepperConfigurationManager configurationManager, ModuleService moduleService)
        {
            service = _service;
            data = configurationManager.currenyConfig;
            ModuleInfo = moduleService.ModuleInfo[typeof(CurrencyCommands)];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var requireProfile = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireProfileAttribute) != null;
            if(requireProfile)
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username, false);

                if (profile == null)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = "You need a profile to run this command",
                        Footer = BotService.GetEmbedFooter("Use the `currency profile` command to create a profile"),
                        Color = ModuleInfo.Color
                    }).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
            }

            var jobCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RequireJobAttribute);
            if(jobCheck != null)
            {
                var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
                if (profile.Job.Equals("None") && ((RequireJobAttribute)jobCheck).require)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = "You need a job to run this command",
                        Footer = BotService.GetEmbedFooter("Use the \"currency hire\" command to get a job"),
                        Color = ModuleInfo.Color
                    }).ConfigureAwait(false);

                    throw new CustomCommandException();
                }
                else if(!profile.Job.Equals("None") && !((RequireJobAttribute)jobCheck).require)
                {
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Description = "You should not have a job to run this command",
                        Footer = BotService.GetEmbedFooter("Use the `currency resign` command to resign from your job"),
                        Color = ModuleInfo.Color
                    }).ConfigureAwait(false);

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
                        await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                        {
                            Title = "Chill out",
                            Description = profile.Job == "None" ? $"You just resigned and its only been {timeSpan}, gotta wait {Job.resignTimeSpan} hours before you can apply again" : $"Its only been {timeSpan} since your last shift, {job.CoolDown} hours is the minimum time.",
                            Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
                            Color = ModuleInfo.Color
                        }).ConfigureAwait(false);

                        throw new CustomCommandException();
                    }
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("profile")]
        [Description("Gets the profile of he user")]
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
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
                Color = DiscordColor.Gold
            }.AddField("ID: ", profile.DiscordUserID.ToString())
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

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("profile")]
        [Description("Gets the profile of he user")]
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
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
                Color = DiscordColor.Gold
            }.AddField("ID: ", profile.DiscordUserID.ToString())
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

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("Deposit")]
        [Aliases("dep")]
        [Description("Deposits Money into the bank"), RequireProfile]
        public async Task Deposit(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var difference = profile.CoinsBankMax - profile.CoinsBank;

            if (profile.CoinsBank == profile.CoinsBankMax)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} your bank is full");
                return;
            }
            else if (profile.Coins <= 0)
            {
                await ctx.Channel.SendMessageAsync($"your wallet is empty");
                return;
            }

            if (amount.ToLower().Equals("max"))
            {
                var toDep = System.Math.Min(difference, profile.Coins);

                await service.ChangeCoinsBank(ctx.Member.Id, toDep);
                await service.ChangeCoins(ctx.Member.Id, -toDep);

                await ctx.Channel.SendMessageAsync($"Desposited max({toDep} {data.coinsEmoji})");
            }
            else if (int.TryParse(amount, out int x))
            {
                var toDep = int.Parse(amount);

                if(toDep > difference)
                {
                    await ctx.Channel.SendMessageAsync("You can't deposit more than your bank can hold?");
                    return;
                }

                await service.ChangeCoinsBank(ctx.Member.Id, toDep);
                await service.ChangeCoins(ctx.Member.Id, -toDep);

                await ctx.Channel.SendMessageAsync($"Deposited {toDep} {data.coinsEmoji}");
            }
            else
                await ctx.Channel.SendMessageAsync("Your amount wasn't even a number?");
        }

        [Command("Withdraw")]
        [Aliases("with")]
        [Description("Withdraws Money into the bank"), RequireProfile]
        public async Task Withdraw(CommandContext ctx, string amount)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            if(profile.CoinsBank <= 0)
            {
                await ctx.Channel.SendMessageAsync("Your bank is empty?");
                return;
            }

            if (amount.ToLower().Equals("max"))
            {
                await service.ChangeCoinsBank(ctx.Member.Id, -profile.CoinsBank);
                await service.ChangeCoins(ctx.Member.Id, profile.CoinsBank);

                await ctx.Channel.SendMessageAsync($"Withdrawed max({profile.CoinsBank} {data.coinsEmoji})");
            }
            else if (int.TryParse(amount, out int x))
            {
                var toWith = int.Parse(amount);

                if (toWith > profile.CoinsBank)
                {
                    await ctx.Channel.SendMessageAsync("Don't try to break the bot");
                    return;
                }

                await service.ChangeCoinsBank(ctx.Member.Id, -toWith);
                await service.ChangeCoins(ctx.Member.Id, toWith);

                await ctx.Channel.SendMessageAsync($"Withdrawed {toWith} {data.coinsEmoji}");
            }
            else
                await ctx.Channel.SendMessageAsync("Don't try to break the bot");
        }

        [Command("Lend")]
        [Description("Lend another user money"), RequireProfile]
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

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} lended {coins} {data.coinsEmoji} to {member.Mention}").ConfigureAwait(false);
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

                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} lended {coins} {data.coinsEmoji} to {member.Mention}").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Don't try to break the bot").ConfigureAwait(false);
                return;
            }
        }

        [Command("Daily")]
        [Description("Log in daily to collect some coins")]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Daily(CommandContext ctx)
        {
            var coins = new Random().Next(data.dailyMin, data.dailyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); 

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Daily Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the day.",
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize)
            }).ConfigureAwait(false);
        }

        [Command("Weekly")]
        [Description("Log in weekly to collect some coins")]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Weekly(CommandContext ctx)
        {
            var coins = new Random().Next(data.weeklyMin, data.weeklyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); 

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Weekly Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the week.",
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
            }).ConfigureAwait(false);
        }

        [Command("Monthly")]
        [Description("Log in monthly to collect some coins, for simplcity a month is averegaed to 30 days")]
        [RequireProfile, Cooldown(1, (int)TimeSpanEnum.Day, CooldownBucketType.User)]
        public async Task Monthly(CommandContext ctx)
        {
            var coins = new Random().Next(data.monthlyMin, data.monthlyMax);
            await service.ChangeCoins(ctx.Member.Id, coins).ConfigureAwait(false); ;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Monthly Coins",
                Description = $"{coins} {data.coinsEmoji}, Here are your coins for the monthly.",
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
            }).ConfigureAwait(false);
        }

        [Command("SafeMode")]
        [Aliases("sm")]
        [Description("Toggles safe mode"), RequireProfile]
        public async Task SafeMode(CommandContext ctx)
        {
            await service.ToggleSafeMode(ctx.Member.Id).ConfigureAwait(false);
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);

            await ctx.RespondAsync($"Safe mode set to {profile.SafeMode == 1}").ConfigureAwait(false);
        }

        [Command("JobList")]
        [Description("can't remain unemployed can you?")]
        public async Task JobList(CommandContext ctx)
        {
            var jobs = Job.AllJobs;

            string description = string.Empty;
            int level = (await service.GetProfile(ctx.Member.Id, ctx.Member.Username)).Level;

            for (int i = 0; i < jobs.Length; i++)
            {
                var job = jobs[i];
                description += $"{i + 1}. {(level < job.minLvlNeeded ? data.cross : data.tick)} **{job.Name}** {job.Emoji}\n `Min Level:` {job.minLvlNeeded}\n `Avg wage:` {((job.SucceedMax + job.SucceedMin) / 2)}\n\n";
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Job List",
                Description = description,
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
                Footer = BotService.GetEmbedFooter("Use the \"currency hire\" command to get a job")
            }).ConfigureAwait(false);
        }

        [Command("Hire")]
        [Description("Apply for a job")]
        [RequireProfile, RequireJob(false), CheckWorkDate]
        public async Task Hire(CommandContext ctx, string jobName)
        {
            var job = Job.AllJobs.FirstOrDefault(x => x.Name.ToLower() == jobName.ToLower());
            if (job == null)
                await ctx.Channel.SendMessageAsync("The given job was not found");

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            int level = (await service.GetProfile(ctx.Member.Id, ctx.Member.Username)).Level;

            if (level < job.minLvlNeeded)
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} the min level needed for the job is {job.minLvlNeeded}, you're level is {level}");
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Cogratulations",
                Description = $"{ctx.Member.Mention} you have been hired. \nUse the work command to earn some money.",
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize)
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Resign")]
        [Description("Resign from a job")]
        [RequireProfile, RequireJob]
        public async Task Resign(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var prevJob = profile.Job;

            await service.ChangeJob(ctx.Member.Id, "None");

            var embed = new DiscordEmbedBuilder
            {
                Title = "Resigned",
                Description = $"{ctx.Member.Mention} has resigned from being a {prevJob}",
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize)
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            await service.ChangePreviousWorkData(ctx.Member.Id, DateTime.Now);
        }

        Func<int, int, int> GenerateRandom = (int min, int max) => new Random().Next(min, max);

        [Command("Work")]
        [Description("Want to make some money?")]
        [RequireProfile, RequireJob, CheckWorkDate]
        public async Task Work(CommandContext ctx)
        {
            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);
            var steps = await job.GetWork(ModuleInfo.Color, BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize));     

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
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize)
            };

            await service.ChangeCoins(ctx.Member.Id, money);
            await service.ChangePreviousWorkData(ctx.Member.Id, DateTime.Now);

            await ctx.Channel.SendMessageAsync(embed: completedEmbed).ConfigureAwait(false);
        }

        [Command("Shop")]
        [Description("Diaplays items in the shop")]
        public async Task ShowShop(CommandContext ctx)
        {
            var items = Shop.AllItems;

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Width = data.thumbnailSize,
                Height = data.thumbnailSize,
                Url = ctx.Member.AvatarUrl
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Shop",
                Thumbnail = thumbnail,
                Color = ModuleInfo.Color
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
        [Description("Buy an item")]
        [RequireProfile]
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
                    Height = data.thumbnailSize,
                    Width = data.thumbnailSize,
                    Url = ctx.Member.AvatarUrl
                };

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Purchase Successful",
                    Description = result.message,
                    Thumbnail = thumbnail,
                    Color = ModuleInfo.Color
                };

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
        }

        [Command("Sell")]
        [Description("Sell an item")]
        [RequireProfile]
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
                    Height = data.thumbnailSize,
                    Width = data.thumbnailSize,
                    Url = ctx.Member.AvatarUrl
                };

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Sold item",
                    Description = $"Successfuly sold {quantity} {result.Name}(s) for {quantity * result.SellingPrice}",
                    Thumbnail = thumbnail,
                    Color = ModuleInfo.Color
                };

                await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
            }
        }

        [Command("Gift")]
        [Description("Gift items to other members")]
        [RequireProfile]
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
        [RequireProfile]
        public async Task ShowInventory(CommandContext ctx)
        {
            var items = await service.GetItems(ctx.Member.Id);
            string descripton = string.Empty;

            int index = 1;
            foreach(var item in items)
            {
                descripton += $"{index}. {item.Name}\n  Quantity: {item.Count}\n\n";
                index++;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Member.Username}'s Inventory",
                Description = descripton,
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize)
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Inventory")]
        public async Task ShowOtherInventory(CommandContext ctx, DiscordMember member = null)
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

            var items = await service.GetItems(member.Id);
            string descripton = string.Empty;

            int index = 1;
            foreach (var item in items)
            {
                descripton += $"{index}. {item.Name}\n  Quantity: {item.Count}\n\n";
                index++;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{member.Username}'s Inventory",
                Description = descripton,
                Color = ModuleInfo.Color,
                Thumbnail = BotService.GetEmbedThumbnail(member, data.thumbnailSize)
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Use")]
        [Description("Use an item")]
        [RequireProfile]
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
                    Height = data.thumbnailSize,
                    Width = data.thumbnailSize
                }
            }).ConfigureAwait(false);
        }

        [Command("Meme")]
        [Description("The currency meme command")]
        [RequireProfile, PresenceItem(PresenceData.PresenceCommand.Meme)]
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
                    Color = ModuleInfo.Color
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
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, data.thumbnailSize),
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.Username}"),
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }
    }
}
