using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs.WorkDatas
{
    public sealed class RewriteSentence : WorkData<string>
    {
        private string[] Sentences { get; }

        public RewriteSentence(int _totalTime, int _numberOfTurns, string[] sentences) : base(_totalTime, _numberOfTurns)
        {
            Sentences = sentences;
        }

        public override string GetWork() => Sentences[new Random().Next(0, Sentences.Length)];
    }
}
