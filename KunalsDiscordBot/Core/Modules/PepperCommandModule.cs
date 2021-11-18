using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;

namespace KunalsDiscordBot.Core.Modules
{
    public abstract class PepperCommandModule : BaseCommandModule
    {
        public abstract PepperCommandModuleInfo ModuleInfo { get; protected set; }
    }
}
