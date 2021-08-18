using System;
using KunalsDiscordBot.Core.Configurations.Enums;

namespace KunalsDiscordBot.Core.Configurations.Attributes
{
    public class ConfigDataAttribute : Attribute
    {
        public ConfigValue data { get; set; }
        public ConfigValueSet set { get; set; }

        public ConfigDataAttribute(ConfigValue _data) => data = _data;
        public ConfigDataAttribute(ConfigValueSet _set) => set = _set;
    }
}
