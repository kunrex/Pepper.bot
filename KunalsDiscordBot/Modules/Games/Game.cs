using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace KunalsDiscordBot.Modules.Games
{
    public abstract class Game
    {
        public static Type GetGameByName(string name) => Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(Game))).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }
}
