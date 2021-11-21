using System;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public class MusicModuleData
    {
        public int MaxQueueLength { get; set; }
        public int MaxPlayistCount { get; set; }
        public int InactivityLength { get; set; }
        public int QueuePageLimit { get; set; }

        public DiscordColor Color { get; set; }
    }
}
