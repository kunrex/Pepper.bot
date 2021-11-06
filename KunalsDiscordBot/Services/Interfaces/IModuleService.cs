using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;

using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Modules;

namespace KunalsDiscordBot.Services.Modules
{ 
    public interface IModuleService
    {
        public IEnumerable<string> ModuleNames { get; }
        public Dictionary<ConfigValueSet, PepperCommandModuleInfo> ModuleInfo { get; }
    }
}
