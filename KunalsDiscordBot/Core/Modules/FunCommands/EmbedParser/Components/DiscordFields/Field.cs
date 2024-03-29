﻿using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.CommonComponents;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFields
{
    public class Field : EmbedComponent
    {
        public override string Id => "field";
        public override bool Outer => false;
        protected override Regex Regex { get; set; }

        public Name Name { get; private set; }
        public Value Value { get; private set; }
        public Inline Inline { get; private set; }

        public Field()
        {
            Regex = new Regex("(.*)");
        }

        public async override Task<bool> MatchAndExtract()
        {
            if (!await base.MatchAndExtract())
                return false;

            var components = await new EmbedGenerator().ParseComponents(input);
            if (components.Count > 3 || components.Count < 1)
                return false;

            foreach (var component in components)
                if (!ExtractComponent(component))
                    return false;
            
            return true;
        }

        public override DiscordEmbedBuilder Modify(DiscordEmbedBuilder builder) => builder.AddField(Name == null ? "** **" : Name.Value, Value == null ? "** **" : Value.StringValue, Inline == null ? false : Inline.Value);

        private bool ExtractComponent(EmbedComponent component)
        {
            if (!(component is Name) && !(component is Value) && !(component is Inline))
                return false;

            switch(component.GetType())
            {
                case var x when x == typeof(Name):
                    if (Name != null)
                        return false;

                    Name = (Name)component;
                    break;
                case var x when x == typeof(Value):
                    if (Value != null)
                        return false;

                    Value = (Value)component;
                    break;
                case var x when x == typeof(Inline):
                    if (Inline != null)
                        return false;

                    Inline = (Inline)component;
                    break;
            }

            return true;
        }
    }
}
