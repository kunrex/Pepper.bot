﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        private readonly List<Type> modules;
        public IEnumerable<string> ModuleNames { get => modules.Select(x => x.GetCustomAttribute<GroupAttribute>().Name); }

        public Dictionary<ConfigValueSet, PepperCommandModuleInfo> ModuleInfo { get; }

        public ModuleService()
        {
            modules = new List<Type>();
            ModuleInfo = new Dictionary<ConfigValueSet, PepperCommandModuleInfo>();
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule)) && !x.IsAbstract && x.GetCustomAttribute<GroupAttribute>() != null))
            {
                modules.Add(type);

                var attribute = type.GetCustomAttribute<ConfigDataAttribute>();
                if (attribute == null || ModuleInfo.ContainsKey(attribute.set))
                    continue;

                var info = new PepperCommandModuleInfo();

                var decor = type.GetCustomAttribute<DecorAttribute>();
                info.Color = decor == null ? DiscordColor.Blurple : decor.color;
                info.Emoji = decor.emoji;

                var botPermissions = type.GetCustomAttribute<RequireBotPermissionsAttribute>();
                info.Permissions = botPermissions == null ? Permissions.None : botPermissions.Permissions;

                ModuleInfo.Add(attribute.set, info);
            }
        }
    }
}
