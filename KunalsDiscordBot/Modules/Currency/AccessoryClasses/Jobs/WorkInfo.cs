using System;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Currency.Jobs
{
    public struct WorkInfo
    {
        public DiscordEmbedBuilder embed { get; set; }
        public int timeToDo { get; set; }
        public string correctResult { get; set; }
    }
}
