using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.DialogueHandlers.Steps
{
    public class GuessStep : Step
    {
        private readonly string helpCode;
        private readonly int answer;
        private readonly int numOfHints;

        private DiscordEmbedBuilder.EmbedThumbnail thumbnail;
        private DiscordColor color;

        public GuessStep(string _title, string _content, string _tryAgainMessage, int _tries, int _time, string _helpCode, int _answer, int _numOfHints) : base(_title, _content, _tryAgainMessage, _tries, _time)
        {
            helpCode = _helpCode;
            answer = _answer;
            numOfHints = _numOfHints;
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
            DateTime prevTime = DateTime.Now;

            int prevEntry = -1, hints = numOfHints;

            while (currentTry > 0)
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

                    embed.AddField("Time: ", $"{timeLeft} seconds");

                    message.WithEmbed(embed);
                }
                else
                    message.WithContent($"{title}\n{content}");

                await message.SendAsync(channel).ConfigureAwait(false);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.Author.Id == member.Id && x.Channel.Id == channel.Id, TimeSpan.FromSeconds(timeLeft));

                //any of the return cases
                if (messageResult.TimedOut)
                    return false;
                else if(messageResult.Result.Content.ToLower() == helpCode.ToLower())
                {
                    if(hints == 0)
                        await channel.SendMessageAsync("You don't have any hints left??");

                    if (prevEntry == -1)
                        await channel.SendMessageAsync("You haven't even tried yet??!!");

                    await channel.SendMessageAsync($"{(prevEntry > answer ? $"{prevEntry} was too high" : $"{prevEntry} was too low")}").ConfigureAwait(false);
                    hints--;

                    currentTry--;
                }
                else if (!int.TryParse(messageResult.Result.Content, out int x) || int.Parse(messageResult.Result.Content) != answer)
                {
                    currentTry--;
                    DateTime messageTime = DateTime.Now;

                    int difference = (int)(messageTime - prevTime).TotalSeconds;
                    timeLeft -= difference;

                    prevTime = messageTime;
                    await channel.SendMessageAsync($"{tryAgainMessage}, you get {currentTry} more turn(s)").ConfigureAwait(false);

                    if (int.TryParse(messageResult.Result.Content, out int y))
                        prevEntry = int.Parse(messageResult.Result.Content);
                }
                else
                    return true;
            }

            return false;
        }
    }
}
