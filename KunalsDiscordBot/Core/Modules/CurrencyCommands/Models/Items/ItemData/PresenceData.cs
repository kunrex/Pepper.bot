using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData
{
    public struct PresenceData 
    {
        [Flags]
        public enum PresenceCommand
        {
            Meme = 0,
            Game = 2,
            Code = 4,
            Hunt = 8,
            Fish = 16,
            Sleep = 32
        }

        public readonly PresenceCommand allowedCommands;
        public readonly int minReward;
        public readonly int maxReward;

        public PresenceData(PresenceCommand command, int _minReward = 0, int _maxReward = 0)
        {
            allowedCommands = command;

            minReward = _minReward;
            maxReward = _maxReward;
        }

        public int GetReward() => new Random().Next(minReward, maxReward);
    }
}
