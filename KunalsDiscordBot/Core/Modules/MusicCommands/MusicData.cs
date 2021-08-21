using System;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public class MusicModuleData
    {
        public int maxQueueLength { get; set; }
        public int inactivityLength { get; set; }

        public DiscordColor color { get; set; }
    }
}
