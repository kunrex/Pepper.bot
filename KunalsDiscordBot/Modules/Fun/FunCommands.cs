//System name spaces
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Fun;
using KunalsDiscordBot.DialogueHandlers;
using KunalsDiscordBot.DialogueHandlers.Steps;
using KunalsDiscordBot.Reddit;

namespace KunalsDiscordBot.Modules.Fun
{
    [Group("Fun")]
    [Decor("Lilac", ":game_die:")]
    public class FunCommands : BaseCommandModule
    {
        private static readonly FunData data = System.Text.Json.JsonSerializer.Deserialize<FunData>(File.ReadAllText(Path.Combine("Modules", "Fun", "FunData.json")));

        private readonly RedditApp redditApp;

        public FunCommands(RedditApp reddit) => redditApp = reddit;

        List<Spammer> currentSpammers = new List<Spammer>();

        [Command("Spam")]
        [Description("It Spams a word for \"x\" seconds, you can not start more than 1 spam in 1 channel")]
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
                Description = "Hey " + member.Nickname + " , you are " + number + "% **cool** 😎"
            };

            await ctx.Channel.SendMessageAsync(embed: Embed);
        }

        [Command("BeatBox")]
        [Description("beatboxes")]
        public async Task Rap(CommandContext ctx)
        {
            Random random = new Random();
            int number = random.Next(0, data.BatBox1.Length);

            string message = $"{data.BatBox1[number]} and {data.BatBox2[number]} and ";

            for (int i = 0; i < 10; i++)
            {
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
                await Task.Delay(TimeSpan.FromSeconds(0.5f));
            }

            await ctx.Channel.SendMessageAsync(data.BatBox1[number]);
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

            var random = new Random().Next(0, 15);
            var pp = "8";

            for (int i = 0; i < random; i++)
                pp += "=";

            pp += "D";

            var embed = new DiscordEmbedBuilder
            {
                Title = "Only The Truth",
                Description = $"{other.Username}: {pp}"
            };

            await ctx.Channel.SendMessageAsync(embed: embed);
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

        Func<int, int, int> Generate = delegate (int min, int max)
        {
            return new Random().Next(min, max);
        };

        [Command("LetMeGoogleThatForYou")]
        [Description("For those who can't google things for themselves")]
        [Aliases("Google")]
        public async Task Google(CommandContext ctx, string search) => await ctx.RespondAsync("https://www.google.com/search?q=" + search).ConfigureAwait(false);

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
                UseEmbed = false
            };

            var handler = new DialogueHandler(config,
                new List<Step>
                {
                    new GuessStep("Random Number Generator", "Guess the number I'm thinking off between 1 and 20", "That wasn't the number I was thinking off", 5, 20, "hint", num, 2)
                });

            var completed = await handler.ProcessDialogue().ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync($"{(completed ? "Good job that was the number I'm thinking off" : $"Oops, wll I was thinking off {num}")}").ConfigureAwait(false);
        }

        [Command("meme")]
        [Description("Juicy memes staright from r/memes")]
        public async Task Meme(CommandContext ctx)
        {
            var post = redditApp.GetMeme();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink
            }).ConfigureAwait(false);
        }

        [Command("animals")]
        [Description("Animal pics form r/Animals :dog::cat:")]
        public async Task Animals(CommandContext ctx)
        {
            var post = redditApp.GetAnimals();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink
            }).ConfigureAwait(false);
        }

        [Command("awww")]
        [Description("Cutee pics form r/aww :dog::cat:")]
        public async Task Awww(CommandContext ctx)
        {
            var post = redditApp.GetAww();

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink
            }).ConfigureAwait(false);
        }

        [Command("post")]
        [Description("Get a random post from any subredddit")]
        public async Task Post(CommandContext ctx, string subreddit)
        {
            var post = redditApp.GetRandomPost(subreddit);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = post.Title,
                ImageUrl = post.Listing.URL,
                Url = "https://www.reddit.com" + post.Permalink
            }).ConfigureAwait(false);
        }

        [Command("GhostPresence")]
        [Aliases("ghost")]
        [Description("Make the bot come alive!")]
        public async Task Ghost(CommandContext ctx, DiscordChannel channel = null)
        {
            if(GhostPresence.presences.Find(x => x.guildId == ctx.Guild.Id || x.userID == ctx.Member.Id) != null)
            {
                await ctx.Channel.SendMessageAsync("There already is a ghost presence in this server or you have another presence in another server so point being you can't have 2").ConfigureAwait(false);
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

        private class FunData
        {
            public string[] BatBox1 { get; set; }
            public string[] BatBox2 { get; set; }

            public string[] Sentences { get; set; }
            public string[] MemesPath { get; set; }
        }
    }
}
