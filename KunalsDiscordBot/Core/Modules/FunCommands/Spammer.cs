using System;
using System.Threading.Tasks;

using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Events;

namespace KunalsDiscordBot.Core.Modules.FunCommands
{
    public class Spammer 
    {
        public static int StopTime = 1;

        public Spammer(int timeInSeconds, string messageToSay, DiscordChannel _channel)
        {
            time = timeInSeconds;
            message = messageToSay;
            channel = _channel;
            isSpamming = true;
        }

        public SimpleBotEvent OnSpamEnded { get; private set; }

        public int time { get; private set; }
        public string message {get ; private set;}
        public DiscordChannel channel { get; set; }
       
        private bool isSpamming { get; set; }

        public async Task Spam()
        {
            var startSpan = DateTime.Now;

            while (isSpamming)
            {
                if (DateTime.Now - startSpan > TimeSpan.FromMinutes(StopTime))
                    break;

                await channel.SendMessageAsync(message).ConfigureAwait(false);
                System.Threading.Thread.Sleep(time * 1000);
            }

            OnSpamEnded.Invoke();
        }

        public void StopSpam() => isSpamming = false;
    }
}
