using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class ButtonStep : Step
    {
        private readonly List<DiscordButtonComponent> buttons;

        public ButtonStep(string _title, string _content, int _time, List<DiscordButtonComponent> _buttons) : base(_title, _content, _time)
        {
            buttons = _buttons;
        }

        public override async Task<UseResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed)
        {
            var interactivity = client.GetInteractivity();

            var builder = new DiscordMessageBuilder().AddComponents(buttons);
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

            var messageResult = await interactivity.WaitForButtonAsync(message, member, cancellationSource.Token);

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
                message = messageResult.Result.Id
            };
        }
    }
}
