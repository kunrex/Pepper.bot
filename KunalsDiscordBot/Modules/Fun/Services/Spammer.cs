using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot;

namespace KunalsDiscordBot.Services.Fun
{
    public class Spammer : BotService
    {
        public Spammer(int timeInSeconds, string messageToSay, CommandContext _ctx)
        {
            time = timeInSeconds;
            message = messageToSay;
            ctx = _ctx;
            isSpamming = true;

            Spam();
        }

        public int time { get; private set; }
        public string message {get ; private set;}
        public CommandContext ctx { get; set; }

        private bool isSpamming;

        private async void Spam()
        {
            while (isSpamming)
            {
                await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
                System.Threading.Thread.Sleep(time * 1000);
            }
        }

        public void StopSpam() => isSpamming = false;
    }
}
