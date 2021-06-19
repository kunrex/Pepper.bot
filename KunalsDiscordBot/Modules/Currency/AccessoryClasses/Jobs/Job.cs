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
        public readonly string[] RewriteSentences;
        public readonly Dictionary<string, string> FillInTheBlanks;
        public readonly string[][] RewriteWords;

        public readonly string Emoji;
        public readonly int Time;

        public Job(string name, int minLvl, int failMin, int failMax, int succedMin, int succedMax, string emoji, int time, int validWorkTypes, string[] rewrite, Dictionary<string, string> fill, string[][] rewriteWords)
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
            Time = time;
        }

        public static readonly Job Teacher = new Job("Teacher", 1, 20, 30, 50, 70, ":woman_teacher:", 10, 3,
            new string[]
            {
                "An object in motion will remain in motion in the same direction and speed untill an external force is applied",
                "Field Class today!",
                "Time for attendence kids",
                "Whats the law of conversation of energy?",
                "Remember this random fact cause marks are all that matter"
            },

            new Dictionary<string, string>
            {
                {"Children", "Time For the School Prayer `       `"},
                {"Theorum", "You need to specify the `       ` used to prove the answer"},
                {"Homework", "The due date for the `        ` is next monday"}
            },

            new string[4][]
            {
                new string[] { "pencil", "eraser", "duster", "chalk" },
                new string[] { "Tb", "Physics", "Science", "Sleeping"},
                new string[] { "HW", "Chem", "Math", "Biology"},
                new string[] { "Strict", "Marks", "Hell", "Devil" }
            }
            );

        public static readonly Job[] AllJobs = { Teacher };

        public async Task<WorkInfo> GetWork(int index, DiscordColor color)
        {
            string description = string.Empty, result = string.Empty;

            switch(index)
            {
                case 0:
                    int sentenceIndex = new Random().Next(0, RewriteSentences.Length);
                    var sentence = RewriteSentences[sentenceIndex];

                    description = $"Rewrite the following sentence you have {Time} seconds: \n {sentence}";
                    result = sentence;

                    break;
                case 1:
                    int fillInIndex = new Random().Next(0, FillInTheBlanks.Count);
                    var fill = FillInTheBlanks.ElementAt(fillInIndex);

                    description = $"Fill in the blank: \n {fill.Value}";
                    result = fill.Key;

                    break;
                case 2:
                    int wordsIndex = new Random().Next(0, RewriteWords.Length);
                    var words = string.Empty;

                    foreach (string word in RewriteWords[wordsIndex])
                        words += $"`{word}`\n";

                    description = $"Rewrite these words in the following order\n{words}";
                    result = words;

                    break;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Work for {Name}",
                Description = description,
                Color = color
            };

            await Task.CompletedTask;
            return new WorkInfo { embed = embed, correctResult = result, timeToDo = Time };
        }
    }
}
