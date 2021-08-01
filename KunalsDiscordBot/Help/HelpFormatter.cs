//System name spaces
using System.Linq;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using System.Collections.Generic;
using DSharpPlus.CommandsNext.Attributes;

//Custom name spaces
using KunalsDiscordBot.Attributes;
using System.Reflection;
using System;
using KunalsDiscordBot.Services;

namespace KunalsDiscordBot.Help
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private string Title { get; set; }
        private string Description { get; set; }

        private DiscordColor Color { get; set; }
        private List<FieldData> Fields { get; set; } = new List<FieldData>();

        private FieldData Commands { get; set; }
        private FieldData Aliases { get; set; }
        private FieldData Module { get; set; }
        private FieldData Overloads { get; set; }

        private FieldData Cooldown { get; set; }
        private FieldData BotPerms { get; set; }
        private FieldData UserPerms { get; set; }
        private FieldData ExecutionRequirements { get; set; }

        private bool IsCommand { get; set; } = false;
        private bool IsModule { get; set; } = false;
        private bool IsGeneralHelp { get; set; } = false;

        private string Footer { get; set; } = string.Empty;

        public HelpFormatter(CommandContext ctx) : base(ctx) => Footer = $"User: {ctx.Member.DisplayName} at {DateTime.Now}";

        public override CommandHelpMessage Build()
        {
            var Embed = new DiscordEmbedBuilder
            {
                Title = Title,
                Description = Description,
                Color = Color,
                Footer = BotService.GetEmbedFooter(Footer),
            };

            if (IsModule)
                Embed.AddField(Commands.name, Commands.value, false);

            if (IsGeneralHelp)
            {
                foreach (var field in Fields)
                    Embed.AddField(field.name, field.value, field.inline);
            }
            else
            {
                Embed.AddField(Aliases.name, Aliases.value, false);
                Embed.AddField(Module.name, Module.value, false);

                Embed.AddField(BotPerms.name, BotPerms.value, true);
                Embed.AddField(UserPerms.name, UserPerms.value, true);
            }

            if (IsCommand && !IsModule)
            {
                Embed.AddField(Cooldown.name, Cooldown.value, false);
                Embed.AddField(Overloads.name, Overloads.value, false);
            }

            return new CommandHelpMessage(embed: Embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            IsCommand = true;

            Title = Format(command.Name);
            Description = $"**Description**: {command.Description}\n**Usage**: pep {(command.Parent == null ? "" : command.Parent.Name)} {command.Name} [comand parameters]";

            string aliases = GetAliases(command.Aliases);
            if (aliases == string.Empty || string.IsNullOrWhiteSpace(aliases))
                aliases = "None";

            Aliases = new FieldData { name = "__Aliases__", value = aliases, inline = false};

            string module = command.Parent == null ? "None" : Format(command.Parent.Name);
            Module = new FieldData { name = "__Module__", value = module, inline = false };

            var decor = command.Parent == null ? (DecorAttribute)command.CustomAttributes.FirstOrDefault(x => x is DecorAttribute) : (DecorAttribute)command.Parent.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
            Title += $" {(decor == null ? "" : decor.emoji)}";
            Color = decor == null ? DiscordColor.Blurple : decor.color;

            string overloads = string.Empty;
            for (int i = 0; i < command.Overloads.Count; i++)
                overloads += $"🗕 __Overload {i + 1}__:\n{GetOverload(command.Overloads[i])}\n";
            Overloads = new FieldData { name = "__Overloads (Same command with different parameters)__", value = overloads == string.Empty ? "None" : overloads, inline = false };

            var userPerm = (RequireUserPermissionsAttribute)command.ExecutionChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute);
            var botPerm = (RequireBotPermissionsAttribute)command.ExecutionChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute);

            UserPerms = new FieldData { name = "__User Permissions__", value = $"{(userPerm == null ? "None" : userPerm.Permissions.ToString())}" };
            BotPerms = new FieldData { name = "__Bot Permissions__", value = $"{(botPerm == null ? "None" : botPerm.Permissions.ToString())}" };

            var cooldown = (CooldownAttribute)command.ExecutionChecks.FirstOrDefault(x => x is CooldownAttribute);
            Cooldown = new FieldData
            {
                name = "__Cool down__",
                value = $"{(cooldown == null ? "None" : $"{cooldown.Reset.Days} Days, {cooldown.Reset.Hours} Hours, {cooldown.Reset.Minutes} Minutes")}"
            };

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Command[] commands = subcommands.ToArray();
            if (commands[0].Parent != null)//checks if a module or sub module was specified
            {
                IsModule = true;

                var parent = commands[0].Parent;

                Title = Format(parent.Name);

                Description = $"**Description**: {parent.Description}\n\n";
                Description += $"**Help Usage**: pep help {parent.Name} <command name>\n";
                Description += $"**Command Usage**: pep {parent.Name} <command name> [command parameters]";

                var decor = (DecorAttribute)parent.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
                bool isHighlited = decor == null ? true : decor.isHighlited;
                Title += $" {(decor == null ? "" : decor.emoji)}";

                Color = decor == null ? DiscordColor.Blurple : decor.color;

                var comands = GetAllCommands(commands, isHighlited);
                Commands = new FieldData { name = "__Commands__", value = comands == string.Empty ? "None" : comands, inline = false };

                string aliases = GetAliases(parent.Aliases, isHighlited);
                if (aliases == string.Empty || string.IsNullOrWhiteSpace(aliases))
                    aliases = "None";

                Aliases = new FieldData { name = "__Aliases__", value = aliases, inline = false};
                Module = new FieldData { name = "__Module__", value = "None", inline = false};

                var userPerm = (RequireUserPermissionsAttribute)parent.ExecutionChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute);
                var botPerm = (RequireBotPermissionsAttribute)parent.ExecutionChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute);

                UserPerms = new FieldData { name = "__User Permissions__", value = $"{(userPerm == null ? "None" : userPerm.Permissions.ToString())}" };
                BotPerms = new FieldData { name = "__Bot Permissions__", value = $"{(botPerm == null ? "None" : botPerm.Permissions.ToString())}" };
            }
            else
            {
                IsGeneralHelp = true;

                Title = "Help";
                Description = "**Description**: All the modules offered by Pepper \n";
                Description += $"**Help Usage**: pep help <module/command name>\n";

                Color = DiscordColor.Blurple;

                Fields.Add(new FieldData { name = "__Modules__", value = "** **", inline = false });
                foreach (var command in GetAllCommands(commands))
                    Fields.Add(new FieldData { name = command.name, value = command.value, inline = false });

                Fields.Add(new FieldData { name = "** **", value = "** **", inline = false });
                Fields.Add(new FieldData { name = "__Commands__", value = "** **", inline = false });

                var helpCommand = commands.Where(x => x.CustomAttributes.FirstOrDefault(x => x is DecorAttribute) == null).ToList()[0];
                Fields.Add(new FieldData { name = $"• {Format(helpCommand.Name)}", value = $"Description: {helpCommand.Description}", inline = false });
            }

            return this;
        }

        private IEnumerable<FieldData> GetAllCommands(Command[] commands)
        {
            foreach (var command in commands.Where(x => x.CustomAttributes.FirstOrDefault(x => x is DecorAttribute) != null))//get all modules, ignore help command
            {
                var decor = (DecorAttribute)command.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
                yield return new FieldData { name = $"• **{Format(command.Name)}** {(decor == null ? "" : decor.emoji)}\n", value = $"Description: {command.Description}\n\n"};
            }
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

                toString += $"•`{overloadArguments[i].Type.Name} {overloadArguments[i].Name}`, Description: `{(overloadArguments[i].Description == string.Empty || overloadArguments[i].Description == null ? "None" : overloadArguments[i].Description)}`\n";
            }

            return toString == string.Empty ? "No Parameters" : toString;
        }

        private string GetAliases(IReadOnlyList<string> aliases, bool isHighlight = true)
        {
            string aliasesToString = string.Empty;
            string highlight = isHighlight ? "`" : "";

            for (int i = 0; i < aliases.Count; i++)
                aliasesToString += $"{highlight}{aliases[i]}{highlight}{(i == aliases.Count - 1 ? "." : ", ")}";

            return aliasesToString;
        }

        private string Format(string name)
        {
            var str = name;
            str = str.Remove(0, 1);
            
            return str.Insert(0, char.ToUpper(name[0]).ToString());
        }
    }
}
