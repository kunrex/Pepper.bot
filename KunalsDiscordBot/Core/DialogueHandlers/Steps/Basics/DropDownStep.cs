using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class DropDownStep : Step
    {
        private readonly DiscordSelectComponent options;
        private bool disableComponents;

        public DropDownStep(string _title, string _content, int _time, DiscordSelectComponent _options, bool _disableComponents = false) : base(_title, _content, _time)
        {
            options = _options;
            disableComponents = _disableComponents;
        }

        public override async Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var interactivity = client.GetInteractivity();
            var builder = BuildMessage(useEmbed).AddComponents(options);

            var message = await builder.SendAsync(channel);
            var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(time));

            var messageResult = await interactivity.WaitForSelectAsync(message, member, options.CustomId, cancellationSource.Token);

            await messageResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            messageResult.Result.Handled = true;

            if (!cancellationSource.IsCancellationRequested)
                cancellationSource.Cancel();
            cancellationSource.Dispose();

            if(disableComponents)
                await message.ModifyAsync(BuildMessage(useEmbed).AddComponents(new DiscordSelectComponent(options.CustomId, options.Placeholder, new List<DiscordSelectComponentOption>() { }, true)));

            return messageResult.TimedOut ? new DialougeResult
            {
                WasCompleted = false,
                Result = null,
                Message = message
            } : new DialougeResult
            {
                WasCompleted = true,
                Result = string.Join(", ", messageResult.Result.Values),
                Message = message
            };
        }
    }
}
