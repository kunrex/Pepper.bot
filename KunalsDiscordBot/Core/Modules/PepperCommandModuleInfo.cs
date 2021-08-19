using System;
using DSharpPlus;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules
{
    public struct PepperCommandModuleInfo
    {
        public DiscordColor Color { get; set; }
        public Permissions Permissions { get; set; }
    }
}
