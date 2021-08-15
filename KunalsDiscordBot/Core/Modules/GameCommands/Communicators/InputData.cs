using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Games.Communicators
{
    public struct InputData
    {
        public string leaveMessage { get; set; }
        public TimeSpan span { get; set; }

        public string regexMatchFailExpression { get; set; }
        public Func<DiscordMessage, bool> conditions { get; set; }

        public Dictionary<string, (string, string)> extraInputValues { get; set; }
        public Dictionary<string, string> extraOutputValues { get; set; }
    }
}
