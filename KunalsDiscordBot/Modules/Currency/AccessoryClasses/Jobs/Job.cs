using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Currency.Jobs
{
    public class Job
    {
        public readonly string Name;
        public int minLvlNeeded;

        public readonly int FailMin;
        public readonly int FailMax;

        public readonly int SucceedMin;
        public readonly int SucceedMax;

        public readonly int ValidWorkTypes;

        public readonly string Emoji;

        public readonly WorkData<string[]> RewriteSentences;
        public readonly WorkData<Dictionary<string, string>> FillInTheBlanks;
        public readonly WorkData<string[][]> RewriteWords;

        public Job(string name, int minLvl, int failMin, int failMax, int succedMin, int succedMax, string emoji, int validWorkTypes, WorkData<string[]> rewrite,WorkData<Dictionary<string, string>> fill, WorkData<string[][]> rewriteWords)
        {
            Name = name;
            minLvlNeeded = minLvl;

            FailMin = failMin;
            FailMax = failMax;

            SucceedMin = succedMin;
            SucceedMax = succedMax;

            ValidWorkTypes = validWorkTypes;

            RewriteSentences = rewrite;
            FillInTheBlanks = fill;
            RewriteWords = rewriteWords;
            Emoji = emoji;
        }

        public static readonly Job Teacher = new Job("Teacher", 1, 20, 30, 50, 70, ":woman_teacher:", 3,

            new WorkData<string[]>(
                10, 1,
                new string[]
                {
                    "An object in motion will remain in motion in the same direction and speed untill an external force is applied",
                    "Field Class today!",
                    "Time for attendence kids",
                    "Whats the law of conversation of energy?",
                    "Remember this random fact cause marks are all that matter"
                }),

            new WorkData<Dictionary<string, string>>(
                15, 3,
                new Dictionary<string, string>
                {
                     {"Children", "Time For the School Prayer `       `"},
                    {"Theorum", "You need to specify the `       ` used to prove the answer"},
                    {"Homework", "The due date for the `        ` is next monday"}
                }),

            new WorkData<string[][]>(
                10, 1,
                new string[][]
                {
                     new string[] { "pencil", "eraser", "duster", "chalk" },
                    new string[] { "Tb", "Physics", "Science", "Sleeping"},
                    new string[] { "HW", "Chem", "Math", "Biology"},
                    new string[] { "Strict", "Marks", "Hell", "Devil" }
                })
            );

        public static readonly Job[] AllJobs = { Teacher };

        public async Task<WorkInfo> GetWork()
        {
            int index = new Random().Next(1, 3);

            string description = string.Empty, result = string.Empty;
            int tries = 0, time = 0;

            switch(index)
            {
                case 0:
                    int sentenceIndex = new Random().Next(0, RewriteSentences.workData.Length);
                    var sentence = RewriteSentences.workData[sentenceIndex];

                    tries = RewriteSentences.numberOfTurns;
                    time = RewriteSentences.totalTime;

                    description = $"Rewrite the following sentence: \n {sentence}";
                    result = sentence;
                    break;
                case 1:
                    int fillInIndex = new Random().Next(0, FillInTheBlanks.workData.Count);
                    var fill = FillInTheBlanks.workData.ElementAt(fillInIndex);


                    tries = FillInTheBlanks.numberOfTurns;
                    time = FillInTheBlanks.totalTime;

                    description = $"Fill in the blank: \n {fill.Value}";
                    result = fill.Key;
                    break;
                case 2:
                    int wordsIndex = new Random().Next(0, RewriteWords.workData.Length);
                    var words = string.Empty;

                    foreach (string word in RewriteWords.workData[wordsIndex])
                        words += $"`{word}`\n";

                    tries = RewriteWords.numberOfTurns;
                    time = RewriteWords.totalTime;

                    description = $"Rewrite these words in the following order: \n{words}";
                    result = words;

                    break;
            }

            await Task.CompletedTask;
            return new WorkInfo { description = description, correctResult = result, timeToDo = time , tries = tries};
        }
    }
}
