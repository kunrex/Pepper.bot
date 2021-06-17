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

namespace KunalsDiscordBot.Modules.Fun
{
    [Group("Fun")]
    [Decor("Lilac", ":game_die:")]
    public class FunCommands : BaseCommandModule
    {
        private static readonly FunData data = System.Text.Json.JsonSerializer.Deserialize<FunData>(File.ReadAllText(Path.Combine("Modules", "Fun", "FunData.json")));

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
                Title = "Truth",
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

        [Command("meme")]
        [Description("Uploads a random meme made by one the memmbers on the server")]
        public async Task UploadMeme(CommandContext ctx)
        {
            var filePaths = Directory.GetFiles(Path.Combine(data.MemesPath));
            var index = Generate(0, filePaths.Length);

            var filePath = filePaths[index];
            var file = Path.GetFileName(filePath);

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var msg = await new DiscordMessageBuilder()
                        .WithContent("I really don't know")
                        .WithFiles(new Dictionary<string, Stream>() { { file, fs } })
                        .SendAsync(ctx.Channel);

                fs.Close();
            }
        }

        [Command("LetMeGoogleThatForYou")]
        [Description("For those who can't google things for themselves")]
        [Aliases("Google")]
        public async Task Google(CommandContext ctx, string search) => await ctx.RespondAsync("https://www.google.com/search?q=" + search).ConfigureAwait(false);

        private class FunData
        {
            public string[] BatBox1 { get; set; }
            public string[] BatBox2 { get; set; }

            public string[] Sentences { get; set; }
            public string[] MemesPath { get; set; }
        }
    }
}
