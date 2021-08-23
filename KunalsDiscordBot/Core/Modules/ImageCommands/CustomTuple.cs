using System;

namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class CustomTuple<TKey, TValue>
    {
        public TKey Key { get ; private set; }
        public TValue Value { get ; private set; }

        public CustomTuple(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
