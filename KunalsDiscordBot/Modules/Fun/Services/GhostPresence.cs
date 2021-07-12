using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Services.Fun
{
    public class GhostPresence
    {
        private static int presenceTime = 5;
        public static List<GhostData> presences = new List<GhostData>();

        public DiscordChannel dmChannel { get; private set; }
        public DiscordChannel exportChannel { get; private set; }
        public DiscordClient client { get; private set; }

        private InteractivityExtension interactivity;
        private bool completed { get; set; } = false;

        public GhostPresence(DiscordClient _client, DiscordChannel dm, DiscordChannel export, ulong guildID, ulong membedID)
        {
            client = _client;
            interactivity = client.GetInteractivity();

            dmChannel = dm;
            exportChannel = export;

            presences.Add(new GhostData { guildId = guildID, userID = membedID, presence = this });
        }

        public async void BegineGhostPresence()
        {
            await dmChannel.SendMessageAsync("Ghost presence started, ending in 5 minutes").ConfigureAwait(false);

            var input = AwaitExportInput();
            var presence = ContinuePresence();
            var end = AwaitPresneceEnd();

            await Task.WhenAny(input, presence, end);
            await dmChannel.SendMessageAsync("Ghost presence completed").ConfigureAwait(false);

            presences.Remove(presences.Find(x => x.presence == this));
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
