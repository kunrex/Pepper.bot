using System;

namespace KunalsDiscordBot.Core.Attributes.ImageCommands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class WithFileAttribute : Attribute
    {
        public string fileName { get; set; }

        public WithFileAttribute(string name) => fileName = name;
    }
}
