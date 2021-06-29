using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.DialogueHandlers.Steps
{
    public class DisplayStep : Step
    {
        private DiscordEmbedBuilder.EmbedThumbnail thumbnail;
        private DiscordColor color;

        private readonly bool edit;
        private readonly string editedvalue;

        public DisplayStep(string _title, string _content, string _tryAgainMessage, int _tries, int _time, bool editAfter = false, string editted = null) : base(_title, _content, _tryAgainMessage, _tries, _time)
        {
            edit = editAfter;
            editedvalue = editted;
        }

        public override Step WithEmbedData(DiscordColor _color, DiscordEmbedBuilder.EmbedThumbnail _thumbnail)
        {
            thumbnail = _thumbnail;
            color = _color;

            return this;
        }

        public async override Task<bool> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed)
        {
            var message = new DiscordMessageBuilder();
            if (useEmbed)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = content,
                    Color = color,
                    Thumbnail = thumbnail
                };
                message.WithEmbed(embed);
            }
            else
                message.WithContent($"{title}\n{content}");

            var sentMessage = await message.SendAsync(channel).ConfigureAwait(false);

            if (edit)
            {
                await Task.Delay(TimeSpan.FromSeconds(time));

                if (useEmbed)
                {
                    DiscordEmbed editedEmbed = new DiscordEmbedBuilder
                    {
                        Title = title,
                        Description = editedvalue,
                        Color = color,
                        Thumbnail = thumbnail
                    };

                    await sentMessage.ModifyAsync(editedEmbed).ConfigureAwait(false);
                }
                else
                    await sentMessage.ModifyAsync($"{title}\n{editedvalue}").ConfigureAwait(false);
            }

            return true;
        }
    }
}
