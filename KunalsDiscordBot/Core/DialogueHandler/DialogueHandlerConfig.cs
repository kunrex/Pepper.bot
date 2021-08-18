using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public class DialogueHandlerConfig
    {
        public string MainTitle { get; set; }
        public bool UseEmbed { get; set; }

        public DiscordChannel Channel { get; set; }
        public DiscordMember Member { get; set; }
        public DiscordClient Client { get; set; }

        public DialogueHandlerConfig()
        {

        }
    }
}
