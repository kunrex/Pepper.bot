using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

using DSharpPlus.Entities;

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

            Console.WriteLine(input);
            var components = await new EmbedGenerator().ParseComponents(input);
            Console.WriteLine(components.Count);
            if (components.Count > 3 || components.Count < 1)
                return false;

            Console.WriteLine("inside field " + components.Count);
            if (components[0] is Name name)
                Name = name;
            else
                return false;

            Console.WriteLine("inside field " + Name.Value);
            if (components[1] is Value value)
                Value = value;
            else
                return false;

            Console.WriteLine("inside field " + Value.StringValue);
            if (components.Count == 3)
            {
                if (components[2] is Inline inline)
                    Inline = inline;
                else
                    return false;

                Console.WriteLine("inside field " + Inline.Value);
            }
            
            return true;
        }
    }
}
