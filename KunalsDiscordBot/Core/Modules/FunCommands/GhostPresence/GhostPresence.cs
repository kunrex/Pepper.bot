using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Core.Events;

namespace KunalsDiscordBot.Core.Modules.FunCommands
{
    public sealed class GhostPresence
    {
        private static int presenceTime = 5;

        public DiscordChannel dmChannel { get; private set; }
        public DiscordChannel exportChannel { get; private set; }
        public DiscordClient client { get; private set; }

        private InteractivityExtension interactivity;
        private bool completed { get; set; } = false;

        public SimpleBotEvent OnPresenceEnded { get; private set; } = new SimpleBotEvent();

        public GhostPresence(DiscordClient _client, DiscordChannel dm, DiscordChannel export, ulong guildID, ulong membedID)
        {
            client = _client;
            interactivity = client.GetInteractivity();

            dmChannel = dm;
            exportChannel = export;
        }

        public async Task BegineGhostPresence()
        {
            await dmChannel.SendMessageAsync("Ghost presence started, ending in 5 minutes. enter `cancel` to stop the presence").ConfigureAwait(false);

            var input = AwaitExportInput();
            var presence = ContinuePresence();
            var end = AwaitPresneceEnd();

            await Task.WhenAny(input, presence, end);
            await dmChannel.SendMessageAsync("Ghost presence completed").ConfigureAwait(false);

            OnPresenceEnded.Invoke();
        }

        private async Task AwaitPresneceEnd()
        {
            await Task.Delay(TimeSpan.FromMinutes(presenceTime));

            completed = true;
        }

        private async Task AwaitExportInput()
        {
            DateTime startTime = DateTime.Now;
            TimeSpan span = TimeSpan.FromMinutes(presenceTime);

            while (!completed)
            {
                var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == exportChannel.Id && !x.Author.IsBot, span - (startTime - DateTime.Now)).ConfigureAwait(false);
                if (completed)
                    return;

                await dmChannel.SendMessageAsync($"{message.Result.Author.Username} said: {message.Result.Content}").ConfigureAwait(false);
            }    
        }

        private async Task ContinuePresence()
        {
            interactivity = client.GetInteractivity();
            DateTime startTime = DateTime.Now;
            TimeSpan span = TimeSpan.FromMinutes(presenceTime);

            while (!completed)
            {
                var message = await interactivity.WaitForMessageAsync(x => x.Channel.Id == dmChannel.Id && !x.Author.IsBot, span - (startTime - DateTime.Now)).ConfigureAwait(false);

                if (message.TimedOut)
                    break;
                else if (message.Result.Content.ToLower().Equals("cancel"))
                    break;
                else
                {
                    await exportChannel.SendMessageAsync(message.Result.Content).ConfigureAwait(false);
                }
            }

            completed = true;
        }
    }
}
