//System name spaces
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;

//Custom name spaces
using KunalsDiscordBot.Attributes;
using System.Reflection;

namespace KunalsDiscordBot.Help
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private string commandName { get; set; }
        private string description { get; set; }

        private string aliases { get; set; }
        private string module { get; set; }

        private string overloads { get; set; }

        private bool isModuleOrCommand = true;

        private DiscordColor color { get; set; }

        public HelpFormatter(CommandContext ctx) : base(ctx)
        {

        }

        public override CommandHelpMessage Build()
        {
            var Embed = new DiscordEmbedBuilder
            {
                Title = commandName,
                Description = description,
                Color = color
            };

            if (isModuleOrCommand)
            {
                Embed.AddField("Aliases : ", aliases == string.Empty ? "None" : aliases, false);
                Embed.AddField("Module : ", module == null || module == string.Empty ? "None" : module, false);
            }

            if (overloads != null && overloads != string.Empty)
                Embed.AddField("Overloads:\n", overloads);

            return new CommandHelpMessage(embed: Embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            commandName = Format(command.Name);
            description = $"Description: {command.Description}";

            aliases = string.Empty;

            for (int i = 0; i < command.Aliases.Count; i++)
            {
                aliases += $"{command.Aliases[i]}{(i == command.Aliases.Count - 1 ? "." : ", ")}";
            }

            module = command.Parent == null ? string.Empty : command.Parent.Name;

            var decor = command.Parent == null ? (Decor)command.CustomAttributes.FirstOrDefault(x => x is Decor) : (Decor)command.Parent.CustomAttributes.FirstOrDefault(x => x is Decor);
            commandName += $" {decor.emoji}";

            color = decor == null ? DiscordColor.Blurple : decor.color;

            for (int i = 0; i < command.Overloads.Count; i++)
                overloads += $"{i + 1}.\n{GetOverload(command.Overloads[i])}";

            return this;
        }

        private string GetOverload(CommandOverload overload)
        {
            var overloadArguments = overload.Arguments.ToList();
            var toString = string.Empty;

            for (int i = 0; i < overloadArguments.Count; i++)
            {
                if (overloadArguments[i].Type == typeof(CommandContext))//skip the command context
                    continue;

                toString += $"**  **=>{"  "}Argument {i + 1}\n Name: `{overloadArguments[i].Name}`\n Type: `{overloadArguments[i].Type}`\n Description: `{(overloadArguments[i].Description == string.Empty || overloadArguments[i].Description == null ? "None" : overloadArguments[i].Description)}`\n";
            }

            return toString;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Command[] commands = subcommands.ToArray();

            if (commands[0].Parent != null)//checks if a module or sub module was specified
            {
                var parent = commands[0].Parent;

                commandName = Format(parent.Name);

                description = aliases = string.Empty;

                description = $"Description: {parent.Description}\n";
                description += $"\n Commands:";

                var decor = (Decor)parent.CustomAttributes.FirstOrDefault(x => x is Decor);
                bool isHighlited = decor == null ? true : decor.isHighlited;
                commandName += $" {(decor == null ? "" : decor.emoji)}";

                string highlight = isHighlited ? "`" : "";

                for (int i = 0; i < parent.Aliases.Count; i++)
                    aliases += $"{highlight}{parent.Aliases[i]}{highlight}{(i == parent.Aliases.Count - 1 ? "." : ", ")}";

                if (aliases == string.Empty)
                    aliases = "None";

                description += "\n";
                for (int i = 0; i < commands.Length; i++)
                    description += $"{highlight}{Format(commands[i].Name)}{highlight}{(i == parent.Aliases.Count - 1 ? "." : ", ")}";
            }
            else
            {
                commandName = "Help";
                description = "Description: All the modules offered by Pepper -\n";
                description += $"\n Modules:\n";

                foreach (var command in subcommands)
                {
                    var decor = (Decor)command.CustomAttributes.FirstOrDefault(x => x is Decor);
                    description += $"`{Format(command.Name)}` {(decor == null ? "" : decor.emoji)}\n";
                }

                aliases = string.Empty;
                isModuleOrCommand = false;

                color = DiscordColor.Blurple;
            }

            module = "None";

            return this;
        }

        private string Format(string name) => name.Replace(name[0], char.ToUpper(name[0]));
    }
}
