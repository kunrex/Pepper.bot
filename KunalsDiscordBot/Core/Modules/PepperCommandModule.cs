using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace KunalsDiscordBot.Core.Modules
{
    public abstract class PepperCommandModule : BaseCommandModule
    {
        public abstract PepperCommandModuleInfo ModuleInfo { get; protected set; }
    }
}
