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
    public class ButtonStep : Step
    {
        private readonly List<DiscordButtonComponent> buttons;
        private readonly bool disableComponents;

        public ButtonStep(string _title, string _content, int _time, List<DiscordButtonComponent> _buttons, bool _disableComponents = false) : base(_title, _content, _time)
        {
            buttons = _buttons;
            disableComponents = _disableComponents;
        }

        public override async Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var interactivity = client.GetInteractivity();
            var builder = BuildMessage(useEmbed).AddComponents(buttons); 

            var message = await builder.SendAsync(channel);
            var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(time));

            var messageResult = await interactivity.WaitForButtonAsync(message, member, cancellationSource.Token);

            await messageResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            messageResult.Result.Handled = true;

            if (!cancellationSource.IsCancellationRequested)
                cancellationSource.Cancel();
            cancellationSource.Dispose();

            if (disableComponents)
                await message.ModifyAsync(BuildMessage(useEmbed).AddComponents(buttons.Select(x => x.Disable())));

            return messageResult.TimedOut ? new DialougeResult
            {
                WasCompleted = false,
                Result = null,
                Message = message
            } : new DialougeResult
            {
                WasCompleted = true,
                Result = messageResult.Result.Id,
                Message = message
            };
        }
    }
}
