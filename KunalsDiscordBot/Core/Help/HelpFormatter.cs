﻿using System;
using System.Linq;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Attributes;

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
                embed.AddField(Commands.name, Commands.value, false);

            if (IsGeneralHelp)
                foreach (var field in Fields)
                    embed.AddField(field.name, field.value, field.inline);
            else
            {
                embed.AddField(Aliases.name, Aliases.value, false);
                embed.AddField(Module.name, Module.value, false);

                embed.AddField(BotPerms.name, BotPerms.value, true);
                embed.AddField(UserPerms.name, UserPerms.value, true);
            }

            if (IsCommand && !IsModule)
            {
                embed.AddField(Cooldown.name, Cooldown.value, false);
                embed.AddField(Overloads.name, Overloads.value, false);
            }

            return embed;
        }

        public HelpFormatter WithCommand(Command command)
        {
            IsCommand = true;

            Title = command.Name.Format();
            Description = $"**Description**: {command.Description}\n**Usage**: pep {(command.Parent == null ? "" : command.Parent.Name)} {command.Name} [comand parameters]";

            Aliases = new FieldData { name = "__Aliases__", value = command.Aliases.GetAliases(), inline = false};

            string module = command.Parent == null ? "None" : command.Parent.Name.Format();
            Module = new FieldData { name = "__Module__", value = module, inline = false };

            var decor = command.Parent == null ? (DecorAttribute)command.CustomAttributes.FirstOrDefault(x => x is DecorAttribute) : (DecorAttribute)command.Parent.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
            Title += $" {(decor == null ? "" : decor.emoji)}";
            Color = decor == null ? DiscordColor.Blurple : decor.color;

            Overloads = new FieldData { name = "__Overloads (Same command with different parameters)__", value = command.GetFormattedOverloads(), inline = false };

            var perms = command.GetPermissions();
            UserPerms = perms[0];
            BotPerms = perms[1];

            Cooldown = new FieldData { name = "__Cool Down__", value = command.GetCoolDown() };
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

                Commands = new FieldData { name = "__Commands__", value = commands.GetAllCommands(isHighlited), inline = false };

                Aliases = new FieldData { name = "__Aliases__", value = parent.GetAliases(), inline = false};
                Module = new FieldData { name = "__Module__", value = "None", inline = false};

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

                Fields.Add(new FieldData { name = "__Modules__", value = "** **", inline = false });
                foreach (var command in commands.FormatModules())
                    Fields.Add(new FieldData { name = command.name, value = command.value, inline = false });

                Fields.Add(new FieldData { name = "** **", value = "** **", inline = false });
                Fields.Add(new FieldData { name = "__Commands__", value = "** **", inline = false });

                foreach (var command in commands.GetCommands())
                    Fields.Add(new FieldData { name = $"• {command.Name.Format()}", value = $"Description: {command.Description}", inline = false });
            }

            return this;
        }
    }
}
