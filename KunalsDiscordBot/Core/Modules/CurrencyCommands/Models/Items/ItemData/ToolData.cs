using System;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items.ItemData
{
    public struct ToolData
    { 
        public enum ToolType
        {
            BankSpace
        }

        public readonly int MinBoost;
        public readonly int MaxBoost;
        public readonly ToolType Type;

        public ToolData(ToolType type, int min, int max)
        {
            MinBoost = min;
            MaxBoost = max;

            Type = type;
        }

        public int GetBoost() => new Random().Next(MinBoost, MaxBoost);
    }
}
