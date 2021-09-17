using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public class SpectatorCommunicator : DiscordCommunicator
    {
        public DiscordChannel Channel { get; private set; }

        public SpectatorCommunicator(DiscordChannel channel)
        {
            Channel = channel;
        }

        public async Task<DiscordMessage> SendMessage(DiscordEmbed embed) => await SendEmbedToPlayer(Channel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(Channel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(Channel, message, embed);

        public async Task SendPageinatedMessage(DiscordUser user, List<Page> pages, PaginationButtons buttons, PaginationBehaviour pagination, ButtonPaginationBehavior deletion, TimeSpan timeSpan)
            => await SendPageinatedMessage(Channel, user, pages, buttons, pagination, deletion, timeSpan);
    }
}
