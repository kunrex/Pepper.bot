using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
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

        public static async Task<DiscordMessage> ClearComponents(this DiscordMessage message) => await message.ModifyAsync(new DiscordMessageBuilder().WithContent(message.Content).AddEmbeds(message.Embeds));
    }
}
