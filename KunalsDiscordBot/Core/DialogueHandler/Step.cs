using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public abstract class Step
    {
        protected readonly string content;
        protected readonly string tryAgainMessage;
        protected readonly string title;
        protected readonly int tries;
        protected readonly int time;

        public Step(string _title, string _content, string _tryAgainMessage, int _tries, int _time)
        {
            content = _content;
            title = _title;
            tryAgainMessage = _tryAgainMessage;

            tries = _tries;
            time = _time;
        }

        public abstract Task<bool> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed);

        public abstract Step WithEmbedData(DiscordColor _color, DiscordEmbedBuilder.EmbedThumbnail _thumbnail);
    }
}
