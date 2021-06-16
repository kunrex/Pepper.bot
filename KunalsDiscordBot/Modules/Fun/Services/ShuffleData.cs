using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Services;

namespace KunalsDiscordBot.Services.Fun
{
    public class ShuffleData : BotService
    {
        public ShuffleType type { get; set; }
        public string shuffle { get; set; }
        public CommandContext ctx { get; set; }
        public DiscordUser user { get; set; }
        private float time { get; set; }

        public ShuffleData(CommandContext _ctx, ShuffleType _type, string _shuffle)
        {
            ctx = _ctx;
            type = _type;
            shuffle = _shuffle;
            user = ctx.User;

            time = 15;
        }

        public async Task WaitForShuffle()
        {
            await ctx.Channel.SendMessageAsync($"Shuffle => {shuffle}. Type anything to start").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var message = await interactivity.WaitForMessageAsync(x => x.Author == user, TimeSpan.FromSeconds(60)).ConfigureAwait(false);

            await Inspect();
        }

        private async Task Inspect()
        {
            await ctx.Channel.SendMessageAsync("Inspection time started, 15 second will be provided. Type anything to start").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();
            var message = await interactivity.WaitForMessageAsync(x => x.Author == user, TimeSpan.FromSeconds(time)).ConfigureAwait(false);

            if (message.TimedOut)
            {
                await ctx.Channel.SendMessageAsync("Inspection Time Croseed, +2").ConfigureAwait(false);
                bool completed = await Count(true);

                await ctx.Channel.SendMessageAsync($"{(completed ? "Good Job" : "Sucks for you")}").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Timer Started, Enter anything to stop").ConfigureAwait(false);
                bool completed = await Count();

                await ctx.Channel.SendMessageAsync($"{(completed ? "Good Job" : "Sucks for you")}").ConfigureAwait(false);
            }
        }

        private async Task<bool> Count(bool plus2 = false)
        {
            DateTime date = System.DateTime.Now;

            var interactivity = ctx.Client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Author == user, TimeSpan.FromSeconds(120)).ConfigureAwait(false);

            TimeSpan timeSpan = DateTime.Now - date;

            if (!string.IsNullOrEmpty(message.Result.Content))
            {
                await ctx.Channel.SendMessageAsync($"{user.Mention} => {timeSpan.Minutes} : {timeSpan.Seconds + (plus2 ? 2 : 0)} : {timeSpan.Milliseconds}, {Evaluate()}").ConfigureAwait(false);
                return true;
            }

            await ctx.Channel.SendMessageAsync("2 minutes up").ConfigureAwait(false);

            return false;

            string Evaluate()
            {
                switch (type)
                {
                    case ShuffleType.ThreeBy3:
                        return "3by3";
                    case ShuffleType.TwoBy2:
                        return "2by2";
                }

                return string.Empty;
            }
        }
    }
}
