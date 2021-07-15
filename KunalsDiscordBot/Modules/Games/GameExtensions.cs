using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;

namespace KunalsDiscordBot.Modules.Games
{
    public static class GameExtensions
    {
        public static IList<T> Shuffle<T>(this IList<T> list)
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

            return list;
        }
    }
}
