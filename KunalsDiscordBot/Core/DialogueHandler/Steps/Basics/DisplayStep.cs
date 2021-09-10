using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class DisplayStep : Step
    {
        private readonly bool edit;
        private readonly string editedvalue;

        public DisplayStep(string _title, string _content, int _time, bool editAfter = false, string editted = null) : base(_title, _content, _time)
        {
            edit = editAfter;
            editedvalue = editted;
        }

        public async override Task<UseResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed)
        {
            var message = new DiscordMessageBuilder();
            if (useEmbed)
                message.WithEmbed(new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = content,
                    Color = color,
                    Thumbnail = thumbnail
                });
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

            return new UseResult
            {
                useComplete = true,
                message = string.Empty
            };
        }
    }
}
