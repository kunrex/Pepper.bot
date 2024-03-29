﻿using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DiscordModels;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public abstract class Step
    {
        protected readonly string title, content;
        protected readonly int time;

        protected MessageData MessageData { get; set; }

        public Step(string _title, string _content, int _time)
        {
            title = _title;
            content = _content;

            time = _time;
        }

        public abstract Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default);

        public Step WithMesssageData(MessageData data)
        {
            MessageData = data;

            return this;
        }

        protected DiscordMessageBuilder BuildMessage(bool useEmbed)
        {
            var builder = new DiscordMessageBuilder();

            if (useEmbed)
            {
                var embed = new DiscordEmbedBuilder
                {
                    Title = title,
                    Description = content,
                    Color = MessageData.EmbedSkeleton.Color,
                };

                if (MessageData.EmbedSkeleton.Footer != null)
                    embed.Footer = MessageData.EmbedSkeleton.Footer;
                if (MessageData.EmbedSkeleton.Author != null)
                    embed.Author = MessageData.EmbedSkeleton.Author;
                if (MessageData.EmbedSkeleton.Thumbnail != null)
                    embed.Thumbnail = MessageData.EmbedSkeleton.Thumbnail;

                builder.AddEmbed(embed);
            }
            else
                builder.WithContent($"{title}\n{content}");

            if (MessageData.Reply)
                builder.WithReply(MessageData.ReplyId);

            return builder;
        }
    }
}
