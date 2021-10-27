using System;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DiscordModels
{
    public struct MessageData
    {
        public EmbedSkeleton EmbedSkeleton { get; set; }

        public bool Reply { get; set; }
        public ulong ReplyId { get; set; }
    }
}
