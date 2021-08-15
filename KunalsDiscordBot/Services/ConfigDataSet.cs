using System;
using KunalsDiscordBot.Core.Attributes;

namespace KunalsDiscordBot.Services
{
    public struct ConfigDataSet
    {
        public ConfigValue ConfigData { get; set; }
        public string FieldName { get; set; }
        public string Description { get; set; }

        public string EditCommand { get; set; }
    }
}
