using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class MessageStep : Step
    {
        public MessageStep(string _title, string _content, int _time) : base(_title, _content, _time)
        {

        }

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var interactivity = client.GetInteractivity();
            var builder = BuildMessage(useEmbed);

            var message = await builder.SendAsync(channel);

            var messageResult = await interactivity.WaitForMessageAsync(x => x.Author.Id == member.Id && x.Channel.Id == channel.Id, TimeSpan.FromSeconds(time));

            if (messageResult.TimedOut)
                return new DialougeResult
                {
                    WasCompleted = false,
                    Result = null,
                    Message = message
                };
            else
                return new DialougeResult
                {
                    WasCompleted = true,
                    Result = messageResult.Result.Content,
                    Message = message
                };
        }
    }
}
