using System;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs.WorkDatas
{
    public sealed class FillInTheBlank : WorkData<CustomTuple<string, string>>
    {
        private TupleBag<string, string> Pairs { get; }

        public FillInTheBlank(int _totalTime, int _numberOfTurns, TupleBag<string, string> pairs) : base(_totalTime, _numberOfTurns)
        {
            Pairs = pairs;
        }

        public override CustomTuple<string, string> GetWork() => Pairs[new Random().Next(0, Pairs.Count)];
    }
}
