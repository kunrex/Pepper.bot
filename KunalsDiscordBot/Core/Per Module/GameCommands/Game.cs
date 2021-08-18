using System;
using System.Linq;
using System.Reflection;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public abstract class Game
    {
        public static Type GetGameByName(string name) => Assembly.GetExecutingAssembly().GetTypes()
            .Where(x => !x.IsAbstract && x.IsSubclassOf(typeof(Game))).FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
    }
}
