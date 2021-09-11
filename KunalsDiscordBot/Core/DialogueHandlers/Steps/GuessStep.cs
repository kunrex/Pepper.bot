using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers.Steps
{
    public class GuessStep : Step, ITurnStep
    {
        private readonly string helpCode;
        private readonly int answer;
        private readonly int numOfHints;

        private readonly int tries;
        public int Tries => tries;

        private readonly string tryAgainMessage;
        public string TryAgainMessage => tryAgainMessage;

        public GuessStep(string _title, string _content, int _time, string _tryAgainMessage, int _tries, string _helpCode, int _answer, int _numOfHints) : base(_title, _content, _time)
        {
            helpCode = _helpCode;
            answer = _answer;

            tryAgainMessage = _tryAgainMessage;
            tries = _tries;

            numOfHints = _numOfHints;
        }

        public async override Task<DialougeResult> ProcessStep(DiscordChannel channel, DiscordMember member, DiscordClient client, bool useEmbed, DialougeResult previousResult = default)
        {
            int currentTry = tries, timeLeft = time;
            DateTime prevTime = DateTime.Now;

            int prevEntry = -1, hints = numOfHints;

            while (currentTry > 0)
            {
                var replyStep = new ReplyStep(title, content, time, new List<string>() { helpCode, answer.ToString() }).WithMesssageData(MessageData);
                var result = await replyStep.ProcessStep(channel, member, client, useEmbed);

                //any of the return cases
                if (!result.WasCompleted && result.Result == null)
                    return result;
                else if (result.Result == helpCode)
                {
                    if (hints == 0)
                        await channel.SendMessageAsync("You don't have any hints left??");
                    else if (prevEntry == -1)
                        await channel.SendMessageAsync("You haven't even tried yet??!!");
                    else
                        await channel.SendMessageAsync($"{(prevEntry > answer ? $"{prevEntry} was too high" : $"{prevEntry} was too low")}").ConfigureAwait(false);

                    hints--;
                    currentTry--;
                }
                else if (!int.TryParse(result.Result, out int x) || result.Result != answer.ToString())
                {
                    currentTry--;
                    DateTime messageTime = DateTime.Now;

                    int difference = (int)(messageTime - prevTime).TotalSeconds;
                    timeLeft -= difference;

                    await channel.SendMessageAsync($"{tryAgainMessage}, you now have {currentTry} more turn(s)").ConfigureAwait(false);

                    prevTime = messageTime;
                    if (int.TryParse(result.Result, out x))
                        prevEntry = int.Parse(result.Result);
                }
                else
                    return result;
            }

            return default;
        }
    }
}
