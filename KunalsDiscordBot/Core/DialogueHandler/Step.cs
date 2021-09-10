using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public abstract class Step
    {
        protected readonly string title, content;
        protected readonly int time;

        protected DiscordEmbedBuilder.EmbedThumbnail thumbnail;
        protected DiscordColor color;

        public Step(string _title, string _content, int _time)
        {
            title = _title;
            content = _content;

            time = _time;
        }

        public abstract Task<UseResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed);

        public Step WithEmbedData(DiscordColor _color, DiscordEmbedBuilder.EmbedThumbnail _thumbnail)
        {
            thumbnail = _thumbnail;
            color = _color;

            return this;
        }
    }
}
