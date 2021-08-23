﻿using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;

using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules;

namespace KunalsDiscordBot.Services.Modules
{ 
    public interface IModuleService
    {
        public List<string> ModuleNames { get; }
        public Dictionary<ConfigValueSet, PepperCommandModuleInfo> ModuleInfo { get; }
        public int TotalCommands { get; }
    }
}