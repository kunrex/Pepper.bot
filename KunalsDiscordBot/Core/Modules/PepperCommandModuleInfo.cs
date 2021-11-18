using System;
using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DiscordModels;

namespace KunalsDiscordBot.Core.Modules
{
    public struct PepperCommandModuleInfo
    {
        public string Emoji { get; set; }
        public DiscordColor Color { get; set; }
        public Permissions Permissions { get; set; }
    }
}
