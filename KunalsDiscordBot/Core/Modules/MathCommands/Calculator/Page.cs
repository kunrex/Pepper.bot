using System;
using System.Collections.Generic;

using DSharpPlus.Entities;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Calculator
{
    public sealed class Page
    {
        public List<DiscordComponent[]> Rows { get; private set; }

        public Page()
        {
            Rows = new List<DiscordComponent[]>();
        }

        public Page AddRow(params DiscordComponent[] components)
        {
            Rows.Add(components);

            return this;
        }
    }
}
