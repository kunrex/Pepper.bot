using System;
using System.Collections.Generic;
using System.Linq;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs.WorkDatas
{
    public sealed class WordsInAnOrder : WorkData<string[]>
    {
        private string[] Words { get; }

        public WordsInAnOrder(int _totalTime, int _numberOfTurns, string[] words) : base(_totalTime, _numberOfTurns)
        {
            Words = words;
        }

        public override string[] GetWork()
        {
            var wordsClone = ((string[])Words.Clone()).ToList();
            var words = new string[4];

            for (int i = 0; i < words.Length; i++)
            {
                int index = new Random().Next(0, wordsClone.Count);

                words[i] = wordsClone[index];
                wordsClone.RemoveAt(index);
            }

            return words;
        }
    }
}
