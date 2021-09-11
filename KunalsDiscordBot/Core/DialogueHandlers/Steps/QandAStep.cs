using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public class QandAStep : Step, ITurnStep
    {
        private readonly int tries;
        public int Tries => tries;

        private readonly string tryAgainMessage;
        public string TryAgainMessage => tryAgainMessage;

        private readonly string response;

        public QandAStep(string _title, string _content, int _time, string _tryAgainMessage, int _tries, string _response) : base(_title, _content, _time)
        {
            response = _response;

            tryAgainMessage = _tryAgainMessage;
            tries = _tries;
        }

        public override async Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            int currentTry = tries, timeLeft = time;
            DateTime prevTime = DateTime.Now;

            while (currentTry > 0)
            {
                var replyStep = new ReplyStep(title, content, timeLeft, response).WithMesssageData(MessageData);
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
