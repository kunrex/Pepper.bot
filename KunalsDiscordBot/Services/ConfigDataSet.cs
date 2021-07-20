using System;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Services
{
    public struct ConfigDataSet
    {
        public string editCommand { get; set; }
        public ConfigData data { get; set; }
    }
}
