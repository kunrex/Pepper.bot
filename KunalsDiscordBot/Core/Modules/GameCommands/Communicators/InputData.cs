using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Communicators
{
    public enum InputType
    {
        Message,
        Button,
        Dropdown
    }

    public struct InputData
    {
        public string LeaveMessage { get; set; }
        public TimeSpan Span { get; set; }

        public string RegexMatchFailExpression { get; set; }
        public Func<DiscordMessage, bool> Conditions { get; set; }

        public Dictionary<string, (string, string)> ExtraInputValues { get; set; }
        public Dictionary<string, string> ExtraOutputValues { get; set; }

        public InputType InputType { get; set; }
    }
}
