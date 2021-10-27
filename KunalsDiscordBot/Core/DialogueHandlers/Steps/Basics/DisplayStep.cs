using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class DisplayStep : Step
    {
        private readonly bool edit;
        private readonly string editedvalue;
        private readonly Func<DialougeResult, Task<DiscordMessageBuilder>> displayMessage;

        public DisplayStep(string _title, string _content, int _time, Func<DialougeResult, Task<DiscordMessageBuilder>> _displayMessage, bool editAfter = false, string editted = null) : base(_title, _content, _time)
        {
            edit = editAfter;
            editedvalue = editted;
            displayMessage = _displayMessage;
        }

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var sentMessage = await (await displayMessage.Invoke(previousResult)).SendAsync(channel).ConfigureAwait(false);

            if (edit)
            {
                await Task.Delay(TimeSpan.FromSeconds(time));

                if (useEmbed)
                {
                    var editedEmbed = new DiscordEmbedBuilder
                    {
                        Title = title,
                        Description = editedvalue,
                        Color = MessageData.EmbedSkeleton.Color,
                    };

                    if (MessageData.EmbedSkeleton.Footer != null)
                        editedEmbed.Footer = MessageData.EmbedSkeleton.Footer;
                    if (MessageData.EmbedSkeleton.Author != null)
                        editedEmbed.Author = MessageData.EmbedSkeleton.Author;
                    if (MessageData.EmbedSkeleton.Thumbnail != null)
                        editedEmbed.Thumbnail = MessageData.EmbedSkeleton.Thumbnail;

                    await sentMessage.ModifyAsync((DiscordEmbed)editedEmbed).ConfigureAwait(false);
                }
                else
                    await sentMessage.ModifyAsync($"{title}\n{editedvalue}").ConfigureAwait(false);
            }

            return new DialougeResult
            {
                WasCompleted = true,
                Result = string.Empty,
                Message = sentMessage
            };
        }
    }
}
