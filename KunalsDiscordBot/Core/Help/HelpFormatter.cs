using System;
using System.Linq;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.DiscordModels;

namespace KunalsDiscordBot.Core.Help
{
    public class HelpFormatter 
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

        private bool IsCommand { get; set; } = false;
        private bool IsModule { get; set; } = false;
        private bool IsGeneralHelp { get; set; } = false;

        private string Footer { get; set; } = string.Empty;

        public HelpFormatter(string displayName) => Footer = $"User: {displayName} at {DateTime.Now}";

        public DiscordEmbedBuilder Build()
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = Title,
                Description = Description,
                Color = Color,
            }.WithFooter(Footer);

            if (IsModule)
                embed.AddField(Commands.Name, Commands.Value, false);

            if (IsGeneralHelp)
                foreach (var field in Fields)
                    embed.AddField(field.Name, field.Value, field.Inline);
            else
            {
                embed.AddField(Aliases.Name, Aliases.Value, false);
                embed.AddField(Module.Name, Module.Value, false);

                embed.AddField(BotPerms.Name, BotPerms.Value, true);
                embed.AddField(UserPerms.Name, UserPerms.Value, true);
            }

            if (IsCommand && !IsModule)
            {
                embed.AddField(Cooldown.Name, Cooldown.Value, false);
                embed.AddField(Overloads.Name, Overloads.Value, false);
            }

            return embed;
        }

        public HelpFormatter WithCommand(Command command)
        {
            IsCommand = true;

            Title = command.Name.Format();
            var parent = command.Parent == null ? "" : command.Parent.Name;

            Description = $"**Description**: {command.Description}\n**Command Usage**: pep {parent} {command.Name} [comand parameters]";

            Aliases = new FieldData { Name = "__Aliases__", Value = command.Aliases.GetAliases(), Inline = false};

            string module = command.Parent == null ? "None" : command.Parent.Name.Format();
            Module = new FieldData { Name = "__Module__", Value = module, Inline = false };

            var decor = command.Parent == null ? (DecorAttribute)command.CustomAttributes.FirstOrDefault(x => x is DecorAttribute) : (DecorAttribute)command.Parent.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
            Title += $" {(decor == null ? "" : decor.emoji)}";
            Color = decor == null ? DiscordColor.Blurple : decor.color;

            Overloads = new FieldData { Name = "__Overloads (Same command with different arguments)__", Value = command.GetFormattedOverloads(), Inline = false };

            var perms = command.GetPermissions();
            UserPerms = perms[0];
            BotPerms = perms[1];

            Cooldown = new FieldData { Name = "__Cool Down__", Value = command.GetCoolDown() };
            return this;
        }

        public HelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Command[] commands = subcommands.ToArray();
            if (commands[0].Parent != null)//checks if a module or sub module was specified
            {
                IsModule = true;

                var parent = commands[0].Parent;

                Title = parent.Name.Format();

                Description = $"**Description**: {parent.Description}\n\n **Help Usage**: pep help {parent.Name} <command name>\n" +
                    $"**Command Usage**: pep {parent.Name} <command name> [command parameters]";

                var decor = (DecorAttribute)parent.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
                bool isHighlited = decor == null ? true : decor.isHighlited;
                Title += $" {(decor == null ? "" : decor.emoji)}";
                Color = decor == null ? DiscordColor.Blurple : decor.color;

                Commands = new FieldData { Name = "__Commands__", Value = commands.GetAllCommands(isHighlited), Inline = false };

                Aliases = new FieldData { Name = "__Aliases__", Value = parent.GetAliases(), Inline = false};
                Module = new FieldData { Name = "__Module__", Value = "None", Inline = false};

                var perms = parent.GetPermissions();
                UserPerms = perms[0];
                BotPerms = perms[1];
            }
            else
            {
                IsGeneralHelp = true;

                Title = "Help";
                Description = "**Description**: Modules enabled in this server\n **Help Usage**: pep help <module/command name>";

                Color = DiscordColor.Blurple;

                Fields.Add(new FieldData { Name = "__Modules__", Value = "** **", Inline = false });
                foreach (var command in commands.FormatModules())
                    Fields.Add(new FieldData { Name = command.Name, Value = command.Value, Inline = false });

                Fields.Add(new FieldData { Name = "** **", Value = "** **", Inline = false });
                Fields.Add(new FieldData { Name = "__Commands__", Value = "** **", Inline = false });

                foreach (var command in commands.GetCommands())
                    Fields.Add(new FieldData { Name = $"• {command.Name.Format()}", Value = $"Description: {command.Description}", Inline = false });
            }

            return this;
        }
    }
}
