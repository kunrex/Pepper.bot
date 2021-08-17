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
using KunalsDiscordBot.Services.Fun;
using KunalsDiscordBot.DialogueHandlers;
using KunalsDiscordBot.DialogueHandlers.Steps;
using KunalsDiscordBot.Core.Reddit;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Services.General;
using System.Reflection;
using KunalsDiscordBot.Core.Configurations;
using KunalsDiscordBot.Core.Modules.FunCommands;
using System.Text.RegularExpressions;
using DSharpPlus;
using KunalsDiscordBot.Core.Attributes.GeneralCommands;
using KunalsDiscordBot.Core.Attributes.FunCommands;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;

namespace KunalsDiscordBot.Modules.Fun
{
    [Group("Fun")]
    [Decor("Lilac", ":game_die:"), ConfigData(ConfigValueSet.Fun)]
    public class FunCommands : BaseCommandModule
    {
        private readonly FunData data;
        private static readonly DiscordColor Color = typeof(FunCommands).GetCustomAttribute<DecorAttribute>().color;

        private readonly RedditApp redditApp;
        private readonly IServerService serverService;

        public FunCommands(PepperConfigurationManager configManager, RedditApp reddit, IServerService _serverService)
        {
            data = configManager.funData;
            redditApp = reddit;
            serverService = _serverService;
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigPermsAttribute) != null;

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

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("ToggleNSFW")]
        [Aliases("NSFW")]
        [Description("Changes wether or not NSFW content is allowed in the server")]
        [CheckConfigPerms, ConfigData(ConfigValue.AllowNSFW)]
        public async Task ToggleNSFW(CommandContext ctx, bool toChange)
        {
            await serverService.ToggleNSFW(ctx.Guild.Id, toChange).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow NSFW` to {toChange}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("AllowSpam")]
        [Description("If true, members of the server will be able to run the spam command (don't recommend for large servers)")]
        [CheckConfigPerms, ConfigData(ConfigValue.AllowSpamCommand)]
        public async Task AllowSpam(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleSpamCommand(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow Spam` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("AllowGhost")]
        [Description("If true, members of the server will be able to run the ghost command (don't recommend for large servers)")]
        [CheckConfigPerms, ConfigData(ConfigValue.AllowGhostCommand)]
        public async Task AllowGhost(CommandContext ctx, bool toSet)
        {
            await serverService.ToggleGhostCommand(ctx.Guild.Id, toSet).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Allow Ghost` to {toSet}",
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        List<Spammer> currentSpammers = new List<Spammer>();

        [Command("Spam")]
        [Description("It Spams a word for \"x\" seconds, you can not start more than 1 spam in 1 channel")]
        [CheckAllowSpam]
        public async Task Spam(CommandContext ctx, int timeInterval, [RemainingText] string message)
        {
            Spammer spamer = currentSpammers.Find(x => x.ctx.Channel == ctx.Channel);
            if (spamer != null)
            {
                await ctx.Channel.SendMessageAsync("There already is a spam going on in the current channel");
                return;
            }

            spamer = new Spammer(timeInterval, message, ctx);

            currentSpammers.Add(spamer);
        }

        [Command("StopSpam")]
        [Description("Stops a spam going on in a channel")]
        [CheckAllowSpam]
        public async Task StopSpam(CommandContext ctx)
        {
            Spammer spammer = currentSpammers.Find(x => x.ctx.Channel == ctx.Channel);

            if (spammer != null)
            {
                spammer.StopSpam();
                currentSpammers.Remove(spammer);
                await ctx.Channel.SendMessageAsync("Stopped Spam");
            }
            else
                await ctx.Channel.SendMessageAsync("No spam going on to stop");
        }

        [Command("describe")]
        [Description("Random charecter dexcription in one word")]
        public async Task charecter(CommandContext ctx)
        {
            string[] replies = { "sweat", "memes", "god", "assasin", "simp", "trash", "legend" };
            Random r = new Random();
            int rand = r.Next(0, replies.Length - 1);

            await ctx.Channel.SendMessageAsync(replies[rand]).ConfigureAwait(false);
        }

        [Command("ask")]
        [Description("Ask the bot something")]
        public async Task Ask(CommandContext ctx, [RemainingText] string question)
        {
            string[] reponses = {"Yes", "No", "Come back later", "How about no", "How about yes", "How about i just ignore you", "Depends, is the sky blue?",
            "Indeed", "Whos asking?", "Come back later when Im not busy", "Well yes", "Well no", "God no!", "Depends, is the Earth round (PS it is)"};

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Title = "8ball thingy",
                Thumbnail = BotService.GetEmbedThumbnail(ctx.User, 30),
                Color = Color
            }.AddField("Question", question).AddField("What I say", reponses[new Random().Next(0, reponses.Length)]));
        }

        [Command("Coolrate")]
        [Description("cool rate")]
        public async Task CoolRate(CommandContext ctx, DiscordMember member = null)
        {
            Random random = new Random();
            int number = random.Next(0, 100);

            if (member == null)
                member = ctx.Member;

            var Embed = new DiscordEmbedBuilder
            {
                Title = "Cool Rate",
                Description = "Hey " + member.Nickname + " , you are " + number + "% **cool** 😎",
                Color = Color
            };

            await ctx.Channel.SendMessageAsync(embed: Embed);
        }

        [Command("BeatBox")]
        [Description("beatboxes")]
        public async Task BeatBox(CommandContext ctx)
        {
            Random random = new Random();
            int number = random.Next(0, data.BeatBox1.Length);

            string message = $"{data.BeatBox1[number]} and {data.BeatBox2[number]} and ";

            for (int i = 0; i < 10; i++)
            {
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
            }

            await ctx.Channel.SendMessageAsync(data.BeatBox1[number]);
        }

        [Command("RewriteSentence")]
        [Description("rewrites the message")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            await ctx.Channel.SendMessageAsync("Rewrite the following sentence, you have 10 seconds");

            Random random = new Random();
            int index = random.Next(0, data.Sentences.Length - 1);

            await ctx.Channel.SendMessageAsync(data.Sentences[index]);

            var messsage = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.User).ConfigureAwait(false);
            if (messsage.Result.Content == data.Sentences[index])
                await ctx.Channel.SendMessageAsync("Good job");
            else
                await ctx.Channel.SendMessageAsync("What a bot, you couldnt do that much");
        }

        [Command("PP")]
        [Description("I present the truth, nothing more")]
        public async Task PP(CommandContext ctx, DiscordMember other = null)
        {
            other = other == null ? ctx.Member : other;

            var pp = "8";
            for (int i = 0; i < new Random().Next(0, 15); i++)
                pp += "=";
            pp += "D";

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Only The Truth",
                Description = $"{other.Username}: {pp}",
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("Shuffle")]
        [Description("Random Shuffle or a cube sole")]
        public async Task Shuffle(CommandContext ctx, [RemainingText] string type)
        {
            Func<int, int, int> Generate = (int min, int max) => new Random().Next(min, max);

            string[] moves = { "R", "L", "F", "B", "U", "D", "F'", "B'", "R'", "L'", "U'", "D'" };
            string shuffle = string.Empty;

            ShuffleType shuffleType = Evaluate();

            int num = Generate(shuffleType == ShuffleType.ThreeBy3 ? 10 : 5, shuffleType == ShuffleType.ThreeBy3 ? 20 : 10);

            for (int i = 0; i < num; i++)
                shuffle += moves[Generate(0, moves.Length)] + (i == num - 1 ? "" : ", ");

            ShuffleData data = new ShuffleData(ctx, shuffleType, shuffle);

            await data.WaitForShuffle();

            ShuffleType Evaluate()
            {
                switch (type)
                {
                    case "3by3":
                        return ShuffleType.ThreeBy3;
                    case "2by2":
                        return ShuffleType.TwoBy2;
                }

                return ShuffleType.ThreeBy3;
            }
        }

        [Command("LetMeGoogleThatForYou")]
        [Description("For those who can't google things for themselves")]
        [Aliases("Google")]
        public async Task Google(CommandContext ctx, string search) => await ctx.RespondAsync("http://lmgtfy.com/?q=" + new Regex("[ ]{1,}", RegexOptions.None).Replace(search, "+")).ConfigureAwait(false);

        [Command("Guess")]
        [Description("Guess a random number")]
        public async Task Guess(CommandContext ctx)
        {
            int num = new Random().Next(1, 21);

            var config = new DialogueHandlerConfig
            {
                Channel = ctx.Channel,
                Member = ctx.Member,
                Client = ctx.Client,
                UseEmbed = false,
            };

            var handler = new DialogueHandler(config,
                new List<Step>
                {
                    new GuessStep("Random Number Generator", "Guess the number I'm thinking off between 1 and 20", "That wasn't the number I was thinking off", 5, 20, "hint", num, 2)
                });

            var completed = await handler.ProcessDialogue().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"{(completed ? "Good job that was the number I'm thinking off" : $"Oops, well I was thinking off {num}")}").ConfigureAwait(false);
        }

        [Command("meme")]
        [Description("Juicy memes straight from r/memes")]
        public async Task Meme(CommandContext ctx)
        {
            var profile = await serverService.GetFunData(ctx.Guild.Id).ConfigureAwait(false);
            var post = redditApp.GetMeme(profile.AllowNSFW == 1);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink,
                Footer = BotService.GetEmbedFooter($"Upvotes: {post.UpVotes}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("aww")]
        [Description("Posts that make you go CUTE!, from r/aww")]
        public async Task Awww(CommandContext ctx)
        {
            var post = redditApp.GetAww();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink,
                Footer = BotService.GetEmbedFooter($"Upvotes: {post.UpVotes}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("animals")]
        [Description("Animal posts from r/Animals, :dog: :cat:")]
        public async Task Animals(CommandContext ctx)
        {
            var post = redditApp.GetAnimals();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink,
                Footer = BotService.GetEmbedFooter($"Upvotes: {post.UpVotes}"),
                Color = Color
            }).ConfigureAwait(false);
        }

        [Command("post")]
        [Description("Get a random post from any subredddit")]
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
                allowNSFW = serverProfile.AllowNSFW == 1,
                imagesOnly = useImage,
                filter = filter,
            });

            if (post == null)
                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "Could not find a post in the subreddit with the given filter",
                    Footer = BotService.GetEmbedFooter("The bot does not take into account all the posts only the a few from the top (like 50 or something, i donno)"),
                    Color = Color
                }).ConfigureAwait(false);
            else
            {
                if (post.NSFW)
                    await ctx.Channel.SendMessageAsync("**THE FOLLOWING POST IS NSFW**").ConfigureAwait(false);

                await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Title = post.Title,
                    ImageUrl = post.Listing.URL,
                    Url = "https://www.reddit.com" + post.Permalink,
                    Footer = BotService.GetEmbedFooter($"Upvotes: {post.UpVotes}"),
                    Color = Color
                }).ConfigureAwait(false);
            }
        }

        [Command("GhostPresence")]
        [Aliases("ghost")]
        [Description("Control the bot temporarily"), CheckAllowGhost]
        public async Task Ghost(CommandContext ctx, DiscordChannel channel = null)
        {
            if(GhostPresence.presences.FirstOrDefault(x => x.guildId == ctx.Guild.Id || x.userID == ctx.Member.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("There already is a ghost presence in this server or you have another presence in another server, so point being there can't be 2").ConfigureAwait(false);
                return;
            }

            var dmChannel = await ctx.Member.CreateDmChannelAsync().ConfigureAwait(false);
            var presence = new GhostPresence(ctx.Client, dmChannel, channel == null ? ctx.Channel : channel, ctx.Channel.Id, ctx.Member.Id);

            try
            {
                await ctx.Message.DeleteAsync().ConfigureAwait(false);
            }
            catch
            {
                await dmChannel.SendMessageAsync("I can't delete messages in that server so you might want to?").ConfigureAwait(false);
            }

            presence.BegineGhostPresence();
        }
    }
}
