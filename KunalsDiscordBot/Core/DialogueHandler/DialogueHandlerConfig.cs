using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public class DialogueHandlerConfig
    {
        public bool UseEmbed { get; set; }
        public bool RequireFullCompletion { get; set; }
        public bool QuickStart { get; set; }

        public DiscordChannel Channel { get; set; }
        public DiscordMember Member { get; set; }
        public DiscordClient Client { get; set; }
    }
}
