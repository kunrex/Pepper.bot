using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;

namespace KunalsDiscordBot.Modules.Games.Communicators
{
    public class SpectatorCommunicator : DiscordCommunicator
    {
        public DiscordChannel channel { get; private set; }

        public SpectatorCommunicator(DiscordChannel _channel)
        {
            channel = _channel;
        }

        public override Task<string> Input(InteractivityExtension interactivity, string inputMessage, InputData data) => throw new NotImplementedException();

        public async Task<DiscordMessage> SendMessage(DiscordEmbed embed) => await SendEmbedToPlayer(channel, embed);
        public async Task<DiscordMessage> SendMessage(string message) => await SendMessageToPlayer(channel, message);
        public async Task<DiscordMessage> SendMessage(string message, DiscordEmbed embed) => await SendMessageToPlayer(channel, message, embed);

        public async Task SendPageinatedMessage(DiscordUser user, List<Page> pages, PaginationEmojis emojis, PaginationBehaviour pagination, PaginationDeletion deletion)
            => await SendPageinatedMessage(channel, user, pages, emojis, pagination, deletion, timeSpan);
    }
}
