using System;

namespace KunalsDiscordBot.Core.DiscordModels
{
    public struct FieldData
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public bool Inline { get; set; }
    }
}
