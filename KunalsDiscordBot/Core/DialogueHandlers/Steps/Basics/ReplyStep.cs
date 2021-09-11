using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class ReplyStep : Step
    {
        public List<string> ValidResponses = new List<string>();

        public ReplyStep(string _title, string _content, int _time, List<string> responses) : base(_title, _content, _time) => ValidResponses = responses;

        public ReplyStep(string _title, string _content, int _time, string response) : base(_title, _content, _time) => ValidResponses = new List<string>() { response };

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var interactivity = client.GetInteractivity();
            var builder = BuildMessage(useEmbed);

            var message = await builder.SendAsync(channel);

            var messageResult = await interactivity.WaitForMessageAsync(x => x.Author.Id == member.Id && x.Channel.Id == channel.Id, TimeSpan.FromSeconds(time));

            //any of the return cases
            if (messageResult.TimedOut)
                return new DialougeResult
                {
                    WasCompleted = false,
                    Result = null,
                    Message = message
                };
            else if (ValidResponses.Find(x => x.ToLower() == messageResult.Result.Content.ToLower()) != null)
                return new DialougeResult
                {
                    WasCompleted = true,
                    Result = messageResult.Result.Content,
                    Message = message
                };
            else
                return new DialougeResult
                {
                    WasCompleted = false,
                    Result = messageResult.Result.Content,
                    Message = message
                };
        }
    }
}
