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

        private bool isCommand = true, isModule = true;

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

            if (isCommand || isModule)
            {
                Embed.AddField("Aliases : ", aliases == string.Empty ? "None" : aliases, false);
                Embed.AddField("Module : ", module == null || module == string.Empty ? "None" : module, false);
            }

            if (isCommand)
                Embed.AddField("Overloads:\n", overloads);

            return new CommandHelpMessage(embed: Embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            commandName = Format(command.Name);
            description = $"Description: {command.Description}";

            aliases = GetAliases(command.Aliases);

            module = command.Parent == null ? string.Empty : command.Parent.Name;

            var decor = command.Parent == null ? (Decor)command.CustomAttributes.FirstOrDefault(x => x is Decor) : (Decor)command.Parent.CustomAttributes.FirstOrDefault(x => x is Decor);
            commandName += $" {(decor == null ? "" : decor.emoji)}";

            if (decor == null)
                isCommand = false;

            color = decor == null ? DiscordColor.Blurple : decor.color;

            overloads = string.Empty;
            for (int i = 0; i < command.Overloads.Count; i++)
                overloads += $"{i + 1}.\n{GetOverload(command.Overloads[i])}";

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Command[] commands = subcommands.ToArray();
            isCommand = false;

            if (commands[0].Parent != null)//checks if a module or sub module was specified
            {
                var parent = commands[0].Parent;

                commandName = Format(parent.Name);

                description = aliases = string.Empty;

                description = $"Description: {parent.Description}\n\n Commands:";

                var decor = (Decor)parent.CustomAttributes.FirstOrDefault(x => x is Decor);
                bool isHighlited = decor == null ? true : decor.isHighlited;
                commandName += $" {(decor == null ? "" : decor.emoji)}";

                aliases += GetAliases(parent.Aliases, decor.isHighlited);

                if (aliases == string.Empty)
                    aliases = "None";

                description += "\n";
                description += GetAllCommands(commands, decor.isHighlited);
            }
            else
            {
                isModule = false;

                commandName = "Help";
                description = "Description: All the modules offered by Pepper -\n\n Modules:\n";
                color = DiscordColor.Blurple;

                description += GetAllCommands(commands);

                aliases = string.Empty;
                isCommand = false;
            }

            module = "None";

            return this;
        }

        private string GetAllCommands(Command[] commands)
        {
            string commandsToString = string.Empty;

            foreach (var command in commands)
            {
                var decor = (Decor)command.CustomAttributes.FirstOrDefault(x => x is Decor);
                commandsToString += $"• **{Format(command.Name)}** {(decor == null ? "" : decor.emoji)}\n Description: {command.Description}\n\n";
            }

            return commandsToString;
        }

        private string GetAllCommands(Command[] commands, bool highlited)
        {
            string commandsToString = string.Empty;
            string highlight = highlited ? "`" : "";

            for (int i = 0; i < commands.Length; i++)
                commandsToString += $"{highlight}{Format(commands[i].Name)}{highlight}{(i == commands.Length - 1 ? "." : ", ")}";

            return commandsToString;
        }

        private string GetOverload(CommandOverload overload)
        {
            var overloadArguments = overload.Arguments.ToList();
            var toString = string.Empty;

            for (int i = 0; i < overloadArguments.Count; i++)
            {
                if (overloadArguments[i].Type == typeof(CommandContext))//skip the command context
                    continue;

                toString += $"=>{"  "}Argument {i + 1}\n Name: `{overloadArguments[i].Name}`\n Type: `{overloadArguments[i].Type}`\n Description: `{(overloadArguments[i].Description == string.Empty || overloadArguments[i].Description == null ? "None" : overloadArguments[i].Description)}`\n";
            }

            return toString;
        }

        private string GetAliases(IReadOnlyList<string> aliases, bool isHighlight = true)
        {
            string aliasesToString = string.Empty;
            string highlight = isHighlight ? "`" : "";

            for (int i = 0; i < aliases.Count; i++)
                aliasesToString += $"{highlight}{aliases[i]}{highlight}{(i == aliases.Count - 1 ? "." : ", ")}";

            return aliasesToString;
        }

        private string Format(string name) => name.Replace(name[0], char.ToUpper(name[0]));
    }
}
