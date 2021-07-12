using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using KunalsDiscordBot.DialogueHandlers.Steps;

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
        public readonly int CoolDown;

        public readonly string Emoji;

        public readonly WorkData<string[]> RewriteSentences;
        public readonly WorkData<Dictionary<string, string>> FillInTheBlanks;
        public readonly WorkData<string[][]> RewriteWords;

        public Job(string name, int minLvl, int failMin, int failMax, int succedMin, int succedMax, string emoji, int validWorkTypes, int cooldDown, WorkData<string[]> rewrite,WorkData<Dictionary<string, string>> fill, WorkData<string[][]> rewriteWords)
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
            Emoji = emoji;
        }

        public static readonly Job Teacher = new Job("Teacher", 1, 20, 30, 50, 70, ":woman_teacher:", 3, 3,

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

        public static readonly Job Developer = new Job("Developer", 5, 50, 75, 100, 125, ":desktop:", 3, 5,

           new WorkData<string[]>(
               10, 1,
               new string[]
               {
                    "My code is compiling!",
                    "C# is just Java but better",
                    "Imagine using python lmao",
                    "Gotta love the simplicity in golangs syntax",
                    "Writing discord bots is pretty fun",
                    "C programmers have no class",
                    "HTML is not a porgramming language",
                    "Where's my github repository?!"
               }),

           new WorkData<Dictionary<string, string>>(
               15, 3,
               new Dictionary<string, string>
               {
                    {"PHP", "Why would anyone use `   `"},
                    {"8", "No you genius, a byte is ` ` bits"},
                    {"Github", "Will you make the `      ` repository?"},
                    {"Python", "Ml training in `      ` is peacefull" },
                    {"Unity", "Unreal engine or `     `?" }
               }),

           new WorkData<string[][]>(
               10, 1,
               new string[][]
               {
                     new string[] { "C#", "Java", "Python", "TypeScript" },
                     new string[] { "GameDev", "Databases", "ML", "NoSleep"},
                     new string[] { "Unity", "Unreal", "Ryder", "VS"},
                     new string[] { "Bytes", "bits", "int", "string" }
               })
           );

        public static readonly Job Chef = new Job("Chef", 10, 65, 90, 125, 200, ":cook:", 3, 7,

           new WorkData<string[]>(
               10, 1,
               new string[]
               {
                    "Never salt can to pan!! The hand is the middle man",
                    "Would you like a croissant?",
                    "You should follow Gordon Ramsay",
                    "I won master chef when I was 5",
                    "Cake has to be the best desert",
                    "I'm know for selling things that cost $5 for $50",
                    "No one makes a good biryani like me",
               }),

           new WorkData<Dictionary<string, string>>(
               15, 3,
               new Dictionary<string, string>
               {
                    {"Salt", "Add some `    ` for taste"},
                    {"Lasagne", "No no its pronounced `       `"},
                    {"Sunny side up", "A `            ` is actually a very complicated dish"},
                    {"Subway", "A `      ` is just a fancy sandwich" },
                    {"KFC", "`   ` or McDonalds?" }
               }),

           new WorkData<string[][]>(
               10, 1,
               new string[][]
               {
                     new string[] { "KFC", "McDonalds", "BurgerKing", "None" },
                     new string[] { "Salad", "Vegan", "Fruits", "Vegeies"},
                     new string[] { "Duck", "Ckicken", "Turkey", "Meat"},
                     new string[] { "Salmon", "Pomfret", "Shrimp", "Cavelier" }
               })
           );

        public static readonly Job[] AllJobs = { Teacher, Developer, Chef };

        public async Task<List<Step>> GetWork(DiscordColor color, DiscordEmbedBuilder.EmbedThumbnail thumbnail)
        {
            int index = new Random().Next(1, 3);

            string description = string.Empty, result = string.Empty;
            int tries = 0, time = 0;

            await Task.CompletedTask;

            switch(index)
            {
                case 0:
                    int sentenceIndex = new Random().Next(0, RewriteSentences.workData.Length);
                    var sentence = RewriteSentences.workData[sentenceIndex];

                    tries = RewriteSentences.numberOfTurns;
                    time = RewriteSentences.totalTime;

                    description = $"Rewrite the following sentence: \n {sentence}";
                    result = sentence;

                    return new List<Step>
                    {
                         new TextStep($"Work for {Name}", description, "Thats not the words", tries, time, new List<string> { result })
                            .WithEmbedData(color, thumbnail)
                    };
                case 1:
                    int fillInIndex = new Random().Next(0, FillInTheBlanks.workData.Count);
                    var fill = FillInTheBlanks.workData.ElementAt(fillInIndex);


                    tries = FillInTheBlanks.numberOfTurns;
                    time = FillInTheBlanks.totalTime;

                    description = $"Fill in the blank: \n {fill.Value}";
                    result = fill.Key;

                    return new List<Step>
                    {
                        new TextStep($"Work for {Name}", description, "Thats not the words", tries, time, new List<string> { result })
                            .WithEmbedData(color, thumbnail)
                    };
                case 2:
                    int wordsIndex = new Random().Next(0, RewriteWords.workData.Length);
                    var words = string.Empty;

                    foreach (string word in RewriteWords.workData[wordsIndex])
                        words += $"{word}\n";

                    words.Remove(words.Length - 1);

                    tries = RewriteWords.numberOfTurns;
                    time = RewriteWords.totalTime;

                    description = $"Rewrite these words in the following order: \n`{words}`";
                    result = words;

                    return new List<Step>
                    {
                        new TextStep($"Work for {Name}", description, "Thats not the words", tries, time, new List<string> { result })
                            .WithEmbedData(color, thumbnail)
                    };
            }

            return null;
        }
    }
}
