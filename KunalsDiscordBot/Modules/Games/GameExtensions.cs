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
        public static List<T> AsList<T>(this ReadOnlyCollection<T> reactions)
        {
            List<T> reactionsNeeded = new List<T>();
            foreach (var reaction in reactions)
                    reactionsNeeded.Add(reaction);

            return reactionsNeeded;
        }
    }
}
