using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public class FillInTheBlankStep : Step
    {
        private readonly string response, blank;

        private DiscordEmbedBuilder.EmbedThumbnail thumbnail;
        private DiscordColor color;

        public FillInTheBlankStep(string _title, string _content, string _tryAgainMessage, int _tries, int _time, string _response, string _blank) : base(_title, _content, _tryAgainMessage, _tries, _time)
        {
            response = _response;
            blank = _blank;
        }

        public override Step WithEmbedData(DiscordColor _color, DiscordEmbedBuilder.EmbedThumbnail _thumbnail)
        {
            thumbnail = _thumbnail;
            color = _color;

            return this;
        }

        public async override Task<bool> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed)
        {
            int currentTry = tries, timeLeft = time;
            var interactivity = client.GetInteractivity();
            DateTime prevTime = DateTime.Now; string messageContent = content;

            while (currentTry > 0)
            {
                var message = new DiscordMessageBuilder();

                var index = content.IndexOf(blank);
                messageContent = messageContent.Remove(index, blank.Length).Insert(index, response[tries - currentTry].ToString());

                if (useEmbed)
                {
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = title,
                        Description = messageContent,
                        Color = color,
                        Thumbnail = thumbnail
                    };

                    embed.AddField("Time: ", $"{timeLeft} seconds");

                    message.WithEmbed(embed);
                }
                else
                    message.WithContent($"{title}\n{content}");

                await message.SendAsync(channel);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.Author.Id == member.Id && x.Channel.Id == channel.Id, TimeSpan.FromSeconds(timeLeft));

                //any of the return cases
                if (messageResult.TimedOut)
                    return false;
                else if (response.ToLower() == messageResult.Result.Content.ToLower())
                    return true;
                else
                {
                    currentTry--;
                    DateTime messageTime = DateTime.Now;

                    int difference = (int)(messageTime - prevTime).TotalSeconds;
                    timeLeft -= difference;

                    prevTime = messageTime;
                    await channel.SendMessageAsync($"{tryAgainMessage}, you get {currentTry} more turn(s)");
                }
            }

            return false;
        }
    }
}
