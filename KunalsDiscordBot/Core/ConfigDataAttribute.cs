using System;
namespace KunalsDiscordBot.Core.Attributes
{
    public enum ConfigValue
    {
        EnforcePermissions,
        LogErrors,
        LogMembers,
        WelcomeChannel,
        RuleChannel,
        ModRole,
        MutedRole,
        RuleCount,
        DJEnfore,
        DJRole,
        AllowNSFW,
        AllowSpamCommand,
        AllowGhostCommand,
        Connect4Channel,
        TicTacToeChannel
    }

    public enum ConfigValueSet
    {
        General,
        Moderation,
        Music,
        Fun,
        Games
    }

    public class ConfigDataAttribute : Attribute
    {
        public ConfigValue data { get; set; }
        public ConfigValueSet set { get; set; }

        public ConfigDataAttribute(ConfigValue _data) => data = _data;
        public ConfigDataAttribute(ConfigValueSet _set) => set = _set;
    }
}
