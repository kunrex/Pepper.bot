using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Services.Fun;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.DialogueHandlers;
using KunalsDiscordBot.Core.Modules.FunCommands;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.DialogueHandlers.Steps;
using KunalsDiscordBot.Core.Attributes.FunCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;

namespace KunalsDiscordBot.Modules.Fun
{
    [Group("Fun")]
    [Decor("Lilac", ":game_die:"), ConfigData(ConfigValueSet.Fun)]
    [ModuleLifespan(ModuleLifespan.Transient), Description("Commands for general fun! Troll users and use reddit based commands.")]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.EmbedLinks | Permissions.AccessChannels)]
    public class FunCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly FunModuleData funData;
        private readonly RedditApp redditApp;
        private readonly IServerService serverService;
        private readonly IFunService funService;

        public FunCommands(PepperConfigurationManager configManager, RedditApp reddit, IServerService _serverService, IFunService _funService, IModuleService moduleService)
        {
            funData = configManager.FunData;
            redditApp = reddit;
            serverService = _serverService;
            funService = _funService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Fun];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigigurationPermissionsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & DSharpPlus.Permissions.Administrator) != DSharpPlus.Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var allowGhostCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckAllowGhostAttribute) != null;
            if (allowGhostCheck)
            {
                var profile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.AllowGhostCommand == 0)
                {
                    await ctx.RespondAsync(":x: This server doesn't allow ghost command execution").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var allowSpamCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckAllowSpamAttribute) != null;
            if (allowSpamCheck)
            {
                var profile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.AllowSpamCommand == 0)
                {
                    await ctx.RespondAsync(":x: This server doesn't allow spam command execution").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var allowActCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckAllowActAttribute) != null;
            if (allowActCheck)
            {
                var profile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.AllowActCommand == 0)
                {
                    await ctx.RespondAsync(":x: This server doesn't allow act command execution").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var redditCommand = ctx.Command.CustomAttributes.FirstOrDefault(x => x is RedditCommandAttribute) != null;
            if(redditCommand && !redditApp.Online)
            {
                await ctx.RespondAsync("Reddit app isn't online yet, give it some time").ConfigureAwait(false);
                throw new CustomCommandException();
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("AllowNSFW")]
        [Aliases("NSFW")]
        [Description("Changes wether or not NSFW content is allowed in the server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.AllowNSFW)]
        public async Task ToggleNSFW(CommandContext ctx, bool toChange)
        {
            await serverService.ModifyData(await serverService.GetFunData(ctx.Guild.Id), x => x.AllowNSFW = toChange ? 1 : 0 ).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow NSFW` to {toChange}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("AllowSpam")]
        [Description("If true, members of the server will be able to run the spam command (don't recommend for large servers)")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.AllowSpamCommand)]
        public async Task AllowSpam(CommandContext ctx, bool toSet)
        {
            await serverService.ModifyData(await serverService.GetFunData(ctx.Guild.Id), x => x.AllowSpamCommand = toSet ? 1 : 0).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow Spam` to {toSet}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("AllowGhost")]
        [Description("If true, members of the server will be able to run the ghost command (don't recommend for large servers)")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.AllowGhostCommand)]
        public async Task AllowGhost(CommandContext ctx, bool toSet)
        {
            await serverService.ModifyData(await serverService.GetFunData(ctx.Guild.Id), x => x.AllowGhostCommand = toSet ? 1 : 0).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow Ghost` to {toSet}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("AllowAct")]
        [Description("If true, members of the server will be able to run the act command(don't recommend for large servers)")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.AllowActCommand)]
        public async Task AllowAct(CommandContext ctx, bool toSet)
        {
            await serverService.ModifyData(await serverService.GetFunData(ctx.Guild.Id), x => x.AllowActCommand = toSet ? 1 : 0).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow Act` to {toSet}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("Spam")]
        [Description("It Spams a word for \"x\" seconds, you can not start more than 1 spam in 1 channel")]
        [CheckAllowSpam]
        public async Task Spam(CommandContext ctx, int timeInterval, [RemainingText] string message)
        {
            var spammer = await funService.CreateSpammer(ctx.Guild.Id, message, timeInterval, ctx.Channel);

            if (spammer == null)
            {
                await ctx.Channel.SendMessageAsync("There already is a spam going on in the this server");
                return;
            }

            await spammer.Spam();
        }

        [Command("StopSpam")]
        [Description("Stops a spam going on in a channel")]
        [CheckAllowSpam]
        public async Task StopSpam(CommandContext ctx) => await ctx.Channel.SendMessageAsync(await funService.StopSpammer(ctx.Guild.Id) ? "Stopped Spam" : "There is no spam to stop");

        [Command("ask")]
        [Aliases("8ball")]
        [Description("Ask the bot something")]
        public async Task Ask(CommandContext ctx, [RemainingText] string question)
        {
            string[] reponses = {"Yes", "No", "Come back later", "How about no", "How about yes", "How about i just ignore you", "Depends, is the sky blue?",
            "Indeed", "Whos asking?", "Come back later when Im not busy", "Well yes", "Well no", "God no!", "Depends, is the Earth round (PS it is)"};

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "8ball thingy",
                Color = ModuleInfo.Color
            }.WithThumbnail(ctx.Member.AvatarUrl, 10, 10)
             .AddField("Question", question).AddField("What I say", reponses[new Random().Next(0, reponses.Length)]));
        }

        [Command("Coolrate")]
        [Description("cool rate")]
        public async Task CoolRate(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Cool Rate",
                Description = $"Hey {member.Mention}, you are **{new Random().Next(0, 100)}% cool** 😎",
                Color = ModuleInfo.Color
            });
        }

        [Command("Simprate")]
        [Description("simp rate")]
        public async Task Simprate(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Simp Rate",
                Description = $"{member.Mention} is a {new Random().Next(0, 100)}% simp",
                Color = ModuleInfo.Color
            });
        }

        [Command("Waifurate")]
        [Description("waifu rate, 100% fact")]
        public async Task Waifurate(CommandContext ctx, DiscordMember member = null)
        {
            member = member == null ? ctx.Member : member;

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Waifu Rate",
                Description = $"{member.Mention} is a {new Random().Next(0, 100)}% waifu :weary:",
                Color = ModuleInfo.Color
            });
        }

        [Command("Loverate")]
        [Description("displays how much 2 people love each other, 100% fact")]
        public async Task Simprate(CommandContext ctx, DiscordMember member1, DiscordMember member2) => await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Love Rate",
            Description = $"{member1.Mention} and {member2.Mention} are {new Random().Next(0, 100)}% in love :heart:",
            Color = ModuleInfo.Color
        });

        [Command("Loverate")]
        [Description("displays how much 2 people love each other, 100% fact")]
        public async Task Simprate(CommandContext ctx, string item1, string item2) => await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
        {
            Title = "Love Rate",
            Description = $"{item1} and {item2} are {new Random().Next(0, 100)}% in love :heart:",
            Color = ModuleInfo.Color
        });

        [Command("BeatBox")]
        [Description("beatboxes")]
        public async Task BeatBox(CommandContext ctx)
        {
            Random random = new Random();
            int number = random.Next(0, funData.BeatBox1.Length);

            string message = $"{funData.BeatBox1[number]} and {funData.BeatBox2[number]} and ";

            for (int i = 0; i < 10; i++)
            {
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
            }

            await ctx.Channel.SendMessageAsync(funData.BeatBox1[number]);
        }

        [Command("RewriteSentence")]
        [Aliases("Rewrite"), Description("rewrites a sentence")]
        public Task Response(CommandContext ctx)
        {
            string sentence = funData.Sentences[new Random().Next(0, funData.Sentences.Length)];

            var handler = new DialogueHandler(new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = false,
                QuickStart = true
            }).WithSteps(new List<Step>
                {
                    new QandAStep("Rewrite Sentence", $"`{sentence}`",  10, "That wasn't it", 1, sentence),
                    new DisplayStep(default, default, default, (result) => Task.FromResult(new DiscordMessageBuilder()
                    .WithContent($"{(result.WasCompleted ? "Good job" : $"What a bot, couldn't do that much?")}")
                    .WithReply(ctx.Message.Id)))
                });

            return Task.CompletedTask;
        }

        [Command("PP")]
        [Description("I present the truth, nothing more")]
        public async Task PP(CommandContext ctx, DiscordMember other = null)
        {
            other = other == null ? ctx.Member : other;

            var pp = "8";
            for (int i = 0; i < new Random().Next(1, 15); i++)
                pp += '=';
            pp += "D";

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Only The Truth",
                Description = $"{other.Username}: {pp}",
                Color = ModuleInfo.Color
            }).ConfigureAwait(false);
        }

        [Command("LetMeGoogleThatForYou")]
        [Description("For those who can't google things for themselves")]
        [Aliases("LMGTFY")]
        public async Task Google(CommandContext ctx, [RemainingText] string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                await ctx.RespondAsync("At least give me something to google");
                return;
            }

            await ctx.RespondAsync($"<https://letmegooglethat.com/?q={new Regex("[ ]{1,}", RegexOptions.None).Replace(search, "+")}>").ConfigureAwait(false);
        }

        [Command("Guess")]
        [Description("Guess a random number between 1 and 20")]
        public Task Guess(CommandContext ctx)
        {
            int number = new Random().Next(1, 21);

            var config = new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = false,
                QuickStart = true
            };

            new DialogueHandler(config).WithSteps(new List<Step>
                {
                    new GuessStep("Random Number Generator", $"Guess the number I'm thinking off between 1 and 20",  20, "That wasn't the number I was thinking off", 5, "hint", number, 2),
                    new DisplayStep(default, default, default, (result) => Task.FromResult(new DiscordMessageBuilder()
                    .WithContent($"{(result.WasCompleted ? "Good job that was the number I'm thinking off" : $"Oops, well I was thinking off {number}")}")
                    .WithReply(ctx.Message.Id)))
                });

            return Task.CompletedTask;
        }

        [Command("Subreddit")]
        [Description("Get information on a reddit subreddit"), RedditCommand]
        public async Task Subreddit(CommandContext ctx, string name)
        {
            var subreddit = redditApp.GetSubReddit(name);

            if (subreddit == null)
            {
                await ctx.RespondAsync("Subreddit not found");
                return;
            }

            var funData = await serverService.GetFunData(ctx.Guild.Id);
            if(funData.AllowNSFW == 0)
            {
                await ctx.RespondAsync("Given server had NSFW content, this server does not allow NSFW content");
                return;
            }
            if (!ctx.Channel.IsNSFW)
            {
                await ctx.RespondAsync("Given server had NSFW content, this channel does not allow NSFW content");
                return;
            }

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = subreddit.Name,
                Description = $"**{subreddit.Title}**\n{subreddit.Description}",
                Url = "https://www.reddit.com" + subreddit.URL,
                Color = ModuleInfo.Color
            }.AddField("ID", subreddit.Id, true)
             .AddField("Full Name", subreddit.Fullname, true)
             .AddField("Language", string.IsNullOrWhiteSpace(subreddit.Lang) ? "Unspecified" : subreddit.Lang, true)
             .AddField("Over 18", string.IsNullOrWhiteSpace(subreddit.Over18.ToString()) ? "Unspecified" : subreddit.Over18.ToString(), true)
             .AddField("Active User Count", string.IsNullOrWhiteSpace(subreddit.ActiveUserCount.ToString()) ? "Unspecified" : subreddit.ActiveUserCount.ToString(), true)
             .AddField("Key Color",string.IsNullOrWhiteSpace(subreddit.KeyColor) ? "Unspecified" : subreddit.KeyColor, true)
             .WithThumbnail(subreddit.CommunityIcon, 20, 20));
        }

        [Command("meme")]
        [Description("Juicy memes straight from r/memes"), RedditCommand]
        public async Task Meme(CommandContext ctx)
        {
            var profile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);
            var post = redditApp.GetMeme(profile.AllowNSFW == 1 && ctx.Channel.IsNSFW);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink,
                Color = ModuleInfo.Color
            }.WithFooter($"⬆️ : {post.UpVotes}")).ConfigureAwait(false);
        }

        [Command("aww")]
        [Description("Posts that make you go AWWW!, from r/aww"), RedditCommand]
        public async Task Awww(CommandContext ctx)
        {
            var post = redditApp.GetAww();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink,
                Color = ModuleInfo.Color
            }.WithFooter($"⬆️ : {post.UpVotes}")).ConfigureAwait(false);
        }

        [Command("Showerthoughts")]
        [Description("Things to think about while taking a shower :thinking:, from r/Showerthoughts"), RedditCommand]
        public async Task Showerthought(CommandContext ctx)
        {
            var post = redditApp.GetShowerthought();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                Url = "https://www.reddit.com" + post.Permalink,
                Color = ModuleInfo.Color
            }.WithFooter($"⬆️ : {post.UpVotes}")).ConfigureAwait(false);
        }

        [Command("post")]
        [Description("Get a random post from any subredddit"), RedditCommand]
        public async Task Post(CommandContext ctx, string subRedditname, RedditPostFilter filter = RedditPostFilter.New, bool useImage = false)
        { 
            var subReddit = redditApp.GetSubReddit(subRedditname);
            if(subReddit == null)
            {
                await ctx.RespondAsync("Given subreddit not found").ConfigureAwait(false);
                return;
            }

            var serverProfile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);

            if(serverProfile.AllowNSFW == 0 && subReddit.Over18.Value)
            {
                await ctx.RespondAsync("Given subreddit has NSFW content, this server does not allow NSFW posts").ConfigureAwait(false);
                return;
            }
            if(serverProfile.AllowNSFW == 1 && subReddit.Over18.Value && !ctx.Channel.IsNSFW)
            {
                await ctx.RespondAsync("Given subreddit has NSFW content, this channel does not allow NSFW content").ConfigureAwait(false);
                return;
            }

            var post = redditApp.GetRandomPost(subReddit, new RedditFilter
            {
                AllowNSFW = serverProfile.AllowNSFW == 1 && ctx.Channel.IsNSFW,
                ImagesOnly = useImage,
                Filter = filter,
            });

            if (post == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "Could not find a post in the subreddit with the given filter",
                    Color = ModuleInfo.Color
                }.WithFooter("The bot does not take into account all the posts only the a few from the top (like 50 or something, i donno)")).ConfigureAwait(false);
            else
                await ctx.RespondAsync(post.NSFW ? "**THE FOLLOWING POST IS NSFW**" : "", new DiscordEmbedBuilder
                {
                    Title = post.Title,
                    ImageUrl = post.Listing.URL,
                    Url = "https://www.reddit.com" + post.Permalink,
                    Color = ModuleInfo.Color
                }.WithFooter($"⬆️ : {post.UpVotes}")).ConfigureAwait(false);
        }

        [Command("GhostPresence")]
        [Aliases("ghost")]
        [Description("Control the bot temporarily"), CheckAllowGhost]
        public async Task Ghost(CommandContext ctx, DiscordChannel channel = null)
        {
            channel = channel == null ? ctx.Channel : channel;

            var dmChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);
            var presence = await funService.CreatePresence(ctx.Guild.Id, ctx.User.Id, ctx.Client, dmChannel, channel);

            if(presence == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to create presence, if a presence is already in the server or you have another presence then wait for it to finish or end it respectiveley");
                return;
            }

            await presence.BegineGhostPresence();
        }

        [Command("RockPaperScissor")]
        [Aliases("rps")]
        public async Task RockPaperScissor(CommandContext ctx, RockPaperScissors choice)
        {
            var aiChoice = (RockPaperScissors)new Random().Next(0, 3);
            var start = $"{ctx.Member.DisplayName} chose {choice}, AI chose {aiChoice}.";

            if (aiChoice == choice)
                await ctx.Channel.SendMessageAsync($"{start} Draw!");
            else
                switch (choice)
                {
                    case RockPaperScissors.Rock:
                        if(aiChoice == RockPaperScissors.Paper)
                            await ctx.Channel.SendMessageAsync($"{start} AI wins!");
                        else
                            await ctx.Channel.SendMessageAsync($"{start} {ctx.Member.DisplayName} wins!");
                        break;
                    case RockPaperScissors.Paper:
                        await ctx.Channel.SendMessageAsync($"{start} {((int)choice > (int)aiChoice ? $"{ctx.Member.DisplayName}" : "AI")} wins");
                        break;
                    case RockPaperScissors.Scissors:
                        if (aiChoice == RockPaperScissors.Paper)
                            await ctx.Channel.SendMessageAsync($"{start} {ctx.Member.DisplayName} wins!");
                        else
                            await ctx.Channel.SendMessageAsync($"{start} AI wins!");
                        break;
                }
        }

        [Command("Choose")]
        [Aliases("Pick"), Description("Choose a value from given values")]
        public async Task Choose(CommandContext ctx, [RemainingText] string options)
        {
            var choices = options.Split(',');
            if (choices.Length <= 1)
                await ctx.RespondAsync("You need to give me 2 or more options");
            else
                await ctx.RespondAsync($"I choose: {choices[new Random().Next(0, choices.Length)]}");
        }

        [Command("Act")]
        [RequireBotPermissions(Permissions.ManageWebhooks)]
        [Description("Send a message on behalf of another user"), CheckAllowAct]
        public async Task Act(CommandContext ctx, DiscordMember member, [RemainingText] string message)
        {
            var webhook = await ctx.Channel.CreateWebhookAsync(member.DisplayName, null, "act command execution");
            await webhook.ExecuteAsync(new DiscordWebhookBuilder { AvatarUrl = member.AvatarUrl, Content = message });

            foreach (var hook in await ctx.Channel.GetWebhooksAsync())
                await hook.DeleteAsync();
        }

        [Command("Say")]
        [Description("I repeat what you say")]
        public async Task Say(CommandContext ctx, [RemainingText] string message)
        {
            if(string.IsNullOrEmpty(message))
            {
                await ctx.RespondAsync("At least give me something to say");
                return;
            }

            await ctx.Channel.SendMessageAsync(message);
        }
    }
}
