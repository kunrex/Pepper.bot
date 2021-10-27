using System;
using System.Collections.Generic;

using KunalsDiscordBot.Core.DiscordModels;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs.WorkDatas;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Jobs
{
    public partial class Job
    {
        public static readonly Job Teacher = new Job("Teacher", 1, 20, 30, 50, 70, ":woman_teacher:", 3, 3,

           new RewriteSentence(10, 1,
               new string[]
               {
                    "An object in motion will remain in motion in the same direction and speed until an external force is applied",
                    "Field Class today!",
                    "Time for attendence kids",
                    "Whats the law of conversation of energy?",
                    "Remember this random fact cause marks are all that matter",
                    "The board of administration has decided that class will be conducted on weekends",
                    "Write an essay on the topic: \"Online class is a boon during the pandemic\""
               }),

           new FillInTheBlank(15, 3,
               new TupleBag<string, string>(new List<(string, string)>
               {
                    ("Children", "Time For the School Prayer --------"),
                    ("Theorum", "You need to specify the ------- used to prove the answer"),
                    ("Homework", "The due date for the -------- is next monday")
               })),

           new WordsInAnOrder(
               10, 1,
               new string[] { "pencil", "eraser", "duster", "chalk", "Texbook", "physics", "science", "HW", "chemistry", "math", "biology", "strict", "marks" })
           );


        public static readonly Job Developer = new Job("Developer", 5, 50, 75, 100, 125, ":desktop:", 3, 5,

           new RewriteSentence( 10, 1,
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

           new FillInTheBlank( 15, 3,
               new TupleBag<string, string>(new List<(string, string)>
               {
                    ("PHP", "Why would anyone use ---"),
                    ("8", "No you genius, a byte is - bits"),
                    ("Github", "Will you make the ------ repository?"),
                    ("Python", "Ml training in ------ is peacefull" ),
                    ("Unity", "Unreal engine or -----?" ),
                    ("binary", "Assembly is just readable ------" ),
                    ("Steve Jobs", "Apple was co-founded by ----------" )
               })),

           new WordsInAnOrder( 10, 1,
               new string[] { "C#", "Java", "Python", "TypeScript", "Game Development", "Databases", "ML", "No Sleep", "Unity", "Unreal", "Ryder", "Visual Studio", "Bytes", "bits", "int", "string" })
           );

        public static readonly Job Chief = new Job("Chef", 10, 65, 90, 125, 200, ":cook:", 3, 7,

            new RewriteSentence( 10, 1,
               new string[]
               {
                    "Never salt can to pan!! The hand is the middle man",
                    "Would you like a croissant?",
                    "You should follow Gordon Ramsay",
                    "I won master chef when I was 5",
                    "Cake has to be the simples desert",
                    "I'm now for selling things that cost $5 for $50",
                    "No one makes a good biryani like me",
                    "KFC is the best fast food restaurant",
                    "Vegans are extreme but have a point"
               }),

            new FillInTheBlank( 15, 3,
               new TupleBag<string, string>(new List<(string, string)>
               {
                    ("Salt", "Add some ----for taste"),
                    ("Lasagne", "No no its pronounced -------"),
                    ("Sunny side up", "A ------------- is actually a very complicated dish"),
                    ("Subway", "A ------ is just a fancy sandwich" ),
                    ("KFC", "--- or McDonalds?" ),
                    ("Pizza Hut", "I feel dominos is better than ---------")
               })),

            new WordsInAnOrder( 10, 1,
                new string[] { "KFC", "McDonalds", "BurgerKing", "Idli", "Dosa", "Salad", "Vegan", "Fruits", "Vegeies", "Duck", "Ckicken", "Turkey", "Meat", "Salmon", "Pomfret", "Shrimp", "Cavelier" })
            );


        public static readonly Job[] AllJobs = { Teacher, Developer, Chief };
    }
}
