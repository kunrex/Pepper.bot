using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public abstract class DiscordCommunicator
    {
        public static readonly string quitInputvalue = "QUIT";
        public static readonly string afkInputvalue = "AFK";
        public static readonly string inputFormatNotFollow = "INVALID";
  
        public DiscordCommunicator()
        {

        }

        //modify
        protected async Task<DiscordMessage> ModifyMessage(DiscordMessage message, string newContent) => await message.ModifyAsync(newContent).ConfigureAwait(false);
        protected async Task<DiscordMessage> ModifyMessage(DiscordMessage message, DiscordEmbed embed) => await message.ModifyAsync(null, embed).ConfigureAwait(false);
        protected async Task<DiscordMessage> ModifyMessage(DiscordMessage message, string newContent, DiscordEmbed embed) => await message.ModifyAsync(newContent, embed).ConfigureAwait(false);

        //send
        protected async Task<DiscordMessage> SendMessageToPlayer(DiscordChannel channel, string message) => await channel.SendMessageAsync(message).ConfigureAwait(false);
        protected async Task<DiscordMessage> SendEmbedToPlayer(DiscordChannel channel, DiscordEmbed embed) => await channel.SendMessageAsync(embed).ConfigureAwait(false);
        protected async Task<DiscordMessage> SendMessageToPlayer(DiscordChannel channel, string message, DiscordEmbed embed) => await channel.SendMessageAsync(message, embed).ConfigureAwait(false);

        //interactivity
        protected async Task SendPageinatedMessage(DiscordChannel channel, DiscordUser user, List<Page> pages, PaginationButtons buttons, PaginationBehaviour pagination, ButtonPaginationBehavior deletion, TimeSpan span)
            => await channel.SendPaginatedMessageAsync(user, pages, buttons, pagination, deletion, new CancellationTokenSource(span).Token);

        protected async Task<InteractivityResult<DiscordMessage>> WaitForMessage(InteractivityExtension interactivity, Func<DiscordMessage, bool> conditions, TimeSpan span)
            => await interactivity.WaitForMessageAsync(conditions, span);

        protected async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReaction(InteractivityExtension interactivity, Func<MessageReactionAddEventArgs, bool> conditions, TimeSpan span)
            => await interactivity.WaitForReactionAsync(conditions, span);

        protected async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForButton(InteractivityExtension interactivity, DiscordMessage message, DiscordUser member, TimeSpan span)
        {
            var result = await interactivity.WaitForButtonAsync(message, member, span);

            if (!result.TimedOut)
            {
                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate);
                result.Result.Handled = true;
            }

            return result;
        }

        protected async Task<InteractivityResult<ComponentInteractionCreateEventArgs>> WaitForSelection(InteractivityExtension interactivity, DiscordMessage message, DiscordUser member, string id, TimeSpan span)
        {
            var result = await interactivity.WaitForSelectAsync(message, member, id, span);

            if (!result.TimedOut)
            {
                await result.Result.Interaction.CreateResponseAsync(DSharpPlus.InteractionResponseType.DeferredMessageUpdate);
                result.Result.Handled = true;
            }

            return result;
        }
    }
}
