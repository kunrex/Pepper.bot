using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Services.Modules
{
    public sealed class ModuleService : IModuleService
    {
        private List<Type> modules;
        public List<string> ModuleNames { get => modules.Select(x => x.GetCustomAttribute<GroupAttribute>().Name).ToList(); }

        public Dictionary<ConfigValueSet, PepperCommandModuleInfo> ModuleInfo { get; } 
        public int TotalCommands { get; }

        public ModuleService()
        {
            ModuleInfo = new Dictionary<ConfigValueSet, PepperCommandModuleInfo>();
            modules = new List<Type>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)) && !x.IsAbstract))
            {
                modules.Add(type);
                TotalCommands += type.GetMethods().Where(x => x.GetCustomAttribute<CommandAttribute>() != null).Count();

                var valueSet = type.GetCustomAttribute<ConfigDataAttribute>().set;
                if (ModuleInfo.ContainsKey(valueSet))
                    continue;

                var info = new PepperCommandModuleInfo();

                var decor = type.GetCustomAttribute<DecorAttribute>();
                info.Color = decor == null ? DiscordColor.Blurple : decor.color;

                var botPermissions = type.GetCustomAttribute<RequireBotPermissionsAttribute>();
                info.Permissions = botPermissions == null ? Permissions.None : botPermissions.Permissions;

                ModuleInfo.Add(valueSet, info);
            }
        }
    }
}
