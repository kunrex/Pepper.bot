using System;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public struct DialougeResult
    {
        public bool WasCompleted { get; set; }
        public string Result { get; set; }
        public DiscordMessage Message { get; set; }
    }
}
