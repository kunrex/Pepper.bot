using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs.WorkDatas;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Jobs
{
    public partial class Job
    {
        public static readonly TimeSpan resignTimeSpan = TimeSpan.FromHours(12);

        public readonly string Name;
        public int minLvlNeeded;

        public readonly int FailMin;
        public readonly int FailMax;

        public readonly int SucceedMin;
        public readonly int SucceedMax;

        public readonly int ValidWorkTypes;
        public readonly int CoolDown;

        public readonly string emoji;

        public readonly RewriteSentence RewriteSentences;
        public readonly FillInTheBlank FillInTheBlanks;
        public readonly WordsInAnOrder RewriteWords;

        public Job(string name, int minLvl, int failMin, int failMax, int succedMin, int succedMax, string emoji, int validWorkTypes, int cooldDown, RewriteSentence rewrite, FillInTheBlank fill, WordsInAnOrder rewriteWords)
        {
            Name = name;
            minLvlNeeded = minLvl;

            FailMin = failMin;
            FailMax = failMax;

            SucceedMin = succedMin;
            SucceedMax = succedMax;

            ValidWorkTypes = validWorkTypes;
            CoolDown = cooldDown;

            RewriteSentences = rewrite;
            FillInTheBlanks = fill;
            RewriteWords = rewriteWords;
            this.emoji = emoji;
        }

        public Task<List<Step>> GetWork(DiscordColor color, DiscordEmbedBuilder.EmbedThumbnail thumbnail)
        {
            int index = new Random().Next(1, 3);

            switch(index)
            {
                case 0:
                    var work = RewriteSentences.GetWork();

                    return Task.FromResult(new List<Step>
                    {
                         new QandAStep($"Work for {Name}", $"Rewrite the following sentence: \n {work}",  RewriteSentences.TotalTime, "Thats not the words",  RewriteSentences.numberOfTurns, work)
                            .WithMesssageData(new MessageData{ Color = color, Thumbnail = thumbnail })
                    });
                case 1:
                    var fill = FillInTheBlanks.GetWork();

                    return Task.FromResult(new List<Step>
                    {
                        new FillInTheBlankStep($"Work for {Name}", $"Fill in the blank: \n {fill.Item2}", FillInTheBlanks.TotalTime, "Thats not the words",  FillInTheBlanks.numberOfTurns, fill.Item1, "-")
                            .WithMesssageData(new MessageData{ Color = color, Thumbnail = thumbnail })
                    });
                case 2:
                    var words = RewriteWords.GetWork();

                    return Task.FromResult(new List<Step>
                    {
                        new QandAStep($"Work for {Name}", $"Rewrite these words in the following order: \n{ string.Join("\n", words.Select(x => $"`{x}`")) }",  RewriteWords.TotalTime, "Thats not the words", RewriteWords.numberOfTurns, string.Join("\n", words))
                            .WithMesssageData(new MessageData{ Color = color, Thumbnail = thumbnail })
                    });
                default:
                    return Task.FromResult<List<Step>>(null);
            }
        }
    }
}
