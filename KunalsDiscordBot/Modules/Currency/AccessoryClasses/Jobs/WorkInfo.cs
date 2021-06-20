using System;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Currency.Jobs
{
    public struct WorkInfo
    {
        public string description { get; set; }
        public string correctResult { get; set; }

        public int tries { get; set; }
        public int timeToDo { get; set; }
    }
}
