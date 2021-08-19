using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Services
{
    public sealed class ModuleService
    {
        public Dictionary<ConfigValueSet, PepperCommandModuleInfo> ModuleInfo { get; private set; }

        public ModuleService()
        {
            ModuleInfo = new Dictionary<ConfigValueSet, PepperCommandModuleInfo>();

            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)) && !x.IsAbstract))
            {
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
