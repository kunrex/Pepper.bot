using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public class FillInTheBlankStep : Step, ITurnStep
    {
        private readonly string response, blankSpot;

        private readonly int tries;
        public int Tries => tries;

        private readonly string tryAgainMessage;
        public string TryAgainMessage => tryAgainMessage;

        public FillInTheBlankStep(string _title, string _content, int _time, string _tryAgainMessage, int _tries, string _response, string _blank) : base(_title, _content, _time)
        {
            response = _response;
            blankSpot = _blank;

            tryAgainMessage = _tryAgainMessage;
            tries = _tries;
        }

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            int currentTry = tries, timeLeft = time;
            DateTime prevTime = DateTime.Now; string currentContent = content;

            while (currentTry > 0)
            {
                var index = currentContent.IndexOf(blankSpot);
                currentContent = currentContent.Remove(index, blankSpot.Length).Insert(index, response[tries - currentTry].ToString());

                var replyStep = new ReplyStep(title, currentContent, timeLeft, response).WithMesssageData(MessageData);
                var result = await replyStep.ProcessStep(channel, member, client, useEmbed);

                //any of the return cases
                if ((!result.WasCompleted && result.Result == null) || result.WasCompleted)
                    return result;
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

            return default;
        }
    }
}
