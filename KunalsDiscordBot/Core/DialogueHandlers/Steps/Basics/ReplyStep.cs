using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics
{
    public class ReplyStep : Step
    {
        public List<string> ValidResponses = new List<string>();

        public ReplyStep(string _title, string _content, int _time, List<string> responses) : base(_title, _content, _time) => ValidResponses = responses;

        public ReplyStep(string _title, string _content, int _time, string response) : base(_title, _content, _time) => ValidResponses = new List<string>() { response };

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            var messageStep = new MessageStep(title, content, time).WithMesssageData(MessageData);
            var result = await messageStep.ProcessStep(channel, member, client, useEmbed, previousResult);

            //any of the return cases
            if (!result.WasCompleted)
                return result;
            else if (ValidResponses.Find(x => x.ToLower() == result.Result.ToLower()) != null)
                return result;
            else
            {
                result.WasCompleted = false;
                return result;
            }
        }
    }
}
