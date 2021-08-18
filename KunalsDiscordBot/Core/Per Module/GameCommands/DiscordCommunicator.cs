using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public abstract class DiscordCommunicator
    {
        public static readonly string quitInputvalue = "QUIT";
        public static readonly string afkInputvalue = "AFK";
        public static readonly string inputFormatNotFollow = "INVALID";

        public Regex inputExpression { get; set; }
        public TimeSpan timeSpan { get; set; }

        public DiscordCommunicator()
        {

        }

        public DiscordCommunicator(Regex expression, TimeSpan span)
        {
            inputExpression = expression;
            timeSpan = span;
        }

        protected bool MatchRegex(string input) => inputExpression.IsMatch(input);

        public abstract Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data);

        protected async Task<DiscordMessage> SendMessageToPlayer(DiscordChannel channel, string message) => await channel.SendMessageAsync(message).ConfigureAwait(false);
        protected async Task<DiscordMessage> SendEmbedToPlayer(DiscordChannel channel, DiscordEmbed embed) => await channel.SendMessageAsync(embed).ConfigureAwait(false);
        protected async Task<DiscordMessage> SendMessageToPlayer(DiscordChannel channel, string message, DiscordEmbed embed) => await channel.SendMessageAsync(message, embed).ConfigureAwait(false);

        protected async Task SendPageinatedMessage(DiscordChannel channel, DiscordUser user, List<Page> pages, PaginationEmojis emojis, PaginationBehaviour pagination, PaginationDeletion deletion, TimeSpan span)
            => await channel.SendPaginatedMessageAsync(user, pages, emojis, pagination, deletion, span);

        protected async Task<InteractivityResult<DiscordMessage>> WaitForMessage(InteractivityExtension interactivity, Func<DiscordMessage, bool> conditions, TimeSpan span)
            => await interactivity.WaitForMessageAsync(conditions, span);

        protected async Task<InteractivityResult<MessageReactionAddEventArgs>> WaitForReaction(InteractivityExtension interactivity, Func<MessageReactionAddEventArgs, bool> conditions, TimeSpan span)
            => await interactivity.WaitForReactionAsync(conditions, span);
    }
}
