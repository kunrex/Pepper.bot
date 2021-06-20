//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

//Custom name spaces
using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Currency;
using KunalsDiscordBot.Modules.Currency.Jobs;

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

            var profile = await service.GetProfile(member.Id, member.Username, sameMember);

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
            embed.AddField("Bank: ", $"{profile.CoinsBank} {coinsEmoji} (max: {profile.CoinsBankMax})");


            int level = (int)(MathF.Floor(25 + MathF.Sqrt(625 + 100 * profile.XP)) / 50);
            embed.AddField("Level: ", $"{level}");

            embed.AddField("Job: ", profile.Job);

            await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);
        }

        [Command("Depsoit")]
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

        [Command("JobList")]
        [Description("can't remain unemployed can you?")]
        public async Task JobList(CommandContext ctx)
        {
            var jobs = Job.AllJobs;

            string description = string.Empty;

            for (int i = 0; i < jobs.Length; i++)
            {
                var job = jobs[i];
                description += $"{i + 1}. **{job.Name}** {job.Emoji}\n `Min Level:` {job.minLvlNeeded}\n `Avg wage:` {((job.SucceedMax + job.SucceedMin) / 2)}\n\n";
            }

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = ctx.Member.AvatarUrl,
                Width = 50,
                Height = 50
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Job List",
                Description = description,
                Color = DiscordColor.Gold,
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

            if(!profile.Job.Equals("None"))
            {
                await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} you already have a job, use the resign command to resign");
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
                Width = 50,
                Height = 50
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Cogratulations",
                Description = $"{ctx.Member.Mention} you have been hired. \nUse the work command to earn some money.",
                Color = DiscordColor.Gold,
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
                Width = 50,
                Height = 50
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = "Resigned",
                Description = $"{ctx.Member.Mention} has resigned from being a {prevJob}",
                Color = DiscordColor.Gold,
                Thumbnail = thumbnail
            };

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        Func<int, int, int> GenerateRandom = (int min, int max) => new Random().Next(min, max);

        [Command("Work")]
        [Description("Want to make some money?")]
        public async Task Work(CommandContext ctx)
        {
            var member = ctx.Member;

            var profile = await service.GetProfile(ctx.Member.Id, ctx.Member.Username);
            if (profile.Job.Equals("None"))
            {
                await ctx.Channel.SendMessageAsync("You don't have a job, use the joblist command to list them");
                return;
            }

            var job = Job.AllJobs.FirstOrDefault(x => x.Name == profile.Job);
            var workInfo = await job.GetWork();
            int numOfTries = workInfo.tries, timeToDo = workInfo.timeToDo;

            var interactivity = ctx.Client.GetInteractivity();
            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Height = 50,
                Width = 50,
                Url = ctx.Member.AvatarUrl
            };

            while (numOfTries > 0)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"Work For {profile.Job}",
                    Description = workInfo.description,
                    Color = DiscordColor.Gold,
                    Thumbnail = thumbnail
                };

                embed.AddField("Time: ", $"{timeToDo} seconds");
                await ctx.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);


                DateTime prevTime = DateTime.Now;
                var message = await interactivity.WaitForMessageAsync(x => x.Author == ctx.Member && x.Channel == ctx.Channel, TimeSpan.FromSeconds(timeToDo));

                //any of the return cases
                if (message.TimedOut || message.Result.Content.ToLower().Equals(workInfo.correctResult.ToLower()))
                {
                    int money = message.TimedOut ? GenerateRandom(job.FailMin, job.FailMax) : GenerateRandom(job.SucceedMin, job.SucceedMax);

                    var completedEmbed = new DiscordEmbedBuilder
                    {
                        Title = message.TimedOut ? "Time Out" : "Good Job",
                        Description = message.TimedOut ? $"You recieve only {money} coins" : $"You recieve {money} coins for a job well done",
                        Color = DiscordColor.Gold,
                        Thumbnail = thumbnail
                    };

                    await service.ChangeCoins(ctx.Member.Id, money);
                    await ctx.Channel.SendMessageAsync(embed: completedEmbed).ConfigureAwait(false);
                    return;
                }
                else
                {
                    numOfTries--;
                    DateTime messageTime = DateTime.Now;

                    int difference = (int)(messageTime - prevTime).TotalSeconds;
                    timeToDo -= difference;

                    prevTime = messageTime;
                    await ctx.Channel.SendMessageAsync($"Thats not the right answer, you get {numOfTries} more turn(s)");
                }
            }

            int coins = GenerateRandom(job.FailMin, job.FailMax);

            var faileEmbed = new DiscordEmbedBuilder
            {
                Title = "Time Out",
                Description = $"You recieve only {coins} coins",
                Color = DiscordColor.Gold,
                Thumbnail = thumbnail
            };

            await service.ChangeCoins(ctx.Member.Id, coins);
            await ctx.Channel.SendMessageAsync(embed: faileEmbed).ConfigureAwait(false);
        }
    }
}
