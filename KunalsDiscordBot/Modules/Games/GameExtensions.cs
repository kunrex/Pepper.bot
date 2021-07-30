using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using KunalsDiscordBot.Modules.Games.Complex.UNO;

namespace KunalsDiscordBot.Modules.Games
{
    public static class GameExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < 10; i++)
            {
                var random = new Random();

                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }

            return list;
        }

        public static IList<T> GetElemantsWithIndex<T>(this IList<T> list, int[] indexs)
        {
            var newList = new List<T>();

            for (int i = 0; i < indexs.Length; i++)
                newList.Add(list[indexs[i]]);

            return newList;
        }

        public static bool HasDuplicates<T>(this IList<T> list) => list.Count != list.Distinct().ToList().Count;
    }
}
