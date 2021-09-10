using System;
using System.Threading;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class DropDownStep : Step
    {
        private readonly DiscordSelectComponent options;

        public DropDownStep(string _title, string _content, int _time, DiscordSelectComponent _options) : base(_title, _content, _time)
        {
            options = _options;
        }

        public override async Task<UseResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed)
        {
            var interactivity = client.GetInteractivity();

            var builder = new DiscordMessageBuilder().AddComponents(options);
            if (useEmbed)
                builder.WithEmbed(new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = content,
                    Color = color,
                    Thumbnail = thumbnail
                });
            else
                builder.WithContent($"{title}\n{content}");

            var message = await builder.SendAsync(channel);
            var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(time));

            var messageResult = await interactivity.WaitForSelectAsync(message, member, options.CustomId, cancellationSource.Token);

            await messageResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            messageResult.Result.Handled = true;

            if (!cancellationSource.IsCancellationRequested)
                cancellationSource.Cancel();

            if (messageResult.TimedOut)
                return new UseResult
                {
                    useComplete = false,
                    message = null
                };

            return new UseResult
            {
                useComplete = false,
                message = messageResult.Result.Values[0]
            };
        }
    }
}
