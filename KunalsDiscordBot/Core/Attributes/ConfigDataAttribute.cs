using System;
namespace KunalsDiscordBot.Core.Attributes
{
    public enum ConfigData
    {
        EnforcePermissions,
        LogErrors,
        LogMembers,
        LogChannel,
        ModRole,
        MutedRole,
        RuleChannel,
        RuleCount,
        DJEnfore,
        DJRole,
        AllowNSFW,
    }

    public class ConfigDataAttribute : Attribute
    {
        public ConfigData data { get; set; }

        public ConfigDataAttribute(ConfigData _data) => data = _data;
    }
}
