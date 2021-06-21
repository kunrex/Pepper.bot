using System;
using DSharpPlus.Entities;

using System.Reflection;

namespace KunalsDiscordBot.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class Decor : Attribute 
    {
        public DiscordColor color;
        public bool isHighlited;
        public string emoji;

        public Decor(string colorToSet, string groupEmoji, bool highlited = true)
        {
            var property = typeof(DiscordColor).GetProperty(colorToSet);

            if (property == null)
            {
                color = DiscordColor.Blurple;
                return;
            }

            color = (DiscordColor)property.GetValue(null, null);
            emoji = groupEmoji;
            isHighlited = highlited;
        }
    }
}
