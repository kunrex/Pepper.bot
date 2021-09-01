using System;
using System.Linq;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Help;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Attributes.ModerationCommands;

namespace KunalsDiscordBot.Extensions
{
    public static partial class PepperBotExtensions
    {
        public static Dictionary<string, Command> FilteredRegisteredCommands(this CommandsNextExtension commandsNext)
        {
            var dictionary = new Dictionary<string, Command>();

            foreach (var elemant in commandsNext.RegisteredCommands)
                if (dictionary.FirstOrDefault(x => x.Value.Name == elemant.Value.Name).Value == null)
                    dictionary.Add(elemant.Key, elemant.Value);

            return dictionary;
        }

        public static IEnumerable<Command> GetModules(this IEnumerable<Command> commands) => commands.Where(x => x is CommandGroup);

        public static IEnumerable<Command> GetCommands(this IEnumerable<Command> commands) => commands.Where(x => !(x is CommandGroup));

        public static IEnumerable<FieldData> FormatModules(this Command[] commands)
        {
            foreach (var command in commands.GetModules())//get all modules, ignore help command
            {
                var decor = (DecorAttribute)command.CustomAttributes.FirstOrDefault(x => x is DecorAttribute);
                yield return new FieldData { name = $"• **{command.Name.Format()}** {(decor == null ? "" : decor.emoji)}\n", value = $"Description: {command.Description}\n\n" };
            }
        }

        public static string GetAllCommands(this Command[] commands, bool highlited)
        {
            string commandsToString = string.Empty;
            string highlight = highlited ? "`" : "";

            for (int i = 0; i < commands.Length; i++)
                commandsToString += $"{highlight}{Format(commands[i].Name)}{highlight}{(i == commands.Length - 1 ? "." : ", ")}";

            return commandsToString == string.Empty ? "None" : commandsToString;
        }

        public static FieldData[] GetPermissions(this Command command)
        {
            var userPerm = (RequireUserPermissionsAttribute)command.ExecutionChecks.FirstOrDefault(x => x is RequireUserPermissionsAttribute);
            var botPerm = (RequireBotPermissionsAttribute)command.ExecutionChecks.FirstOrDefault(x => x is RequireBotPermissionsAttribute);

            var requireModerator = (ModeratorNeededAttribute)command.ExecutionChecks.FirstOrDefault(x => x is ModeratorNeededAttribute);
            var userPermsToString = string.Empty;
            if (userPerm != null)
                userPermsToString += userPerm.Permissions.FormatePermissions();
            if (requireModerator != null)
                userPermsToString += $"{(userPerm == null ? "" : ", ")}`Moderator`";

            return new FieldData[]
            {
                new FieldData {name = "__User Permissions__", value = userPermsToString == string.Empty ? "None" : userPermsToString},
                new FieldData {name = "__Bot Permissions__", value = botPerm == null ? "None" : botPerm.Permissions.FormatePermissions()}
            };
        }

        public static string GetCoolDown(this Command command)
        {
            var cooldown = (CooldownAttribute)command.ExecutionChecks.FirstOrDefault(x => x is CooldownAttribute);
            return $"{(cooldown == null ? "None" : $"{cooldown.Reset.Days} Days, {cooldown.Reset.Hours} Hours, {cooldown.Reset.Minutes} Minutes")}";
        }

        public static string FormatePermissions(this Permissions perms) => string.Join(", ", perms.ToString().Replace(" ", "").Split(',').Select(x => $"`{x}`"));

        public static string GetAliases(this IReadOnlyList<string> aliases, bool isHighlight = true)
        {
            string aliasesToString = string.Empty;
            string highlight = isHighlight ? "`" : "";

            for (int i = 0; i < aliases.Count; i++)
                aliasesToString += $"{highlight}{aliases[i]}{highlight}{(i == aliases.Count - 1 ? "." : ", ")}";

            return aliasesToString == string.Empty ? "None" : aliasesToString;
        }

        public static string GetAliases(this CommandGroup group, bool isHighlight = true)
        {
            string aliasesToString = string.Empty;
            string highlight = isHighlight ? "`" : "";

            for (int i = 0; i < group.Aliases.Count; i++)
                aliasesToString += $"{highlight}{group.Aliases[i]}{highlight}{(i == group.Aliases.Count - 1 ? "." : ", ")}";

            return aliasesToString == string.Empty ? "None" : aliasesToString;
        }

        public static string GetFormattedOverloads(this Command command)
        {
            string overloads = string.Empty;

            for (int i = 0; i < command.Overloads.Count; i++)
                overloads += $"🗕 __Overload {i + 1}__:\n{command.Overloads[i].FormattedString()}\n";

            return overloads; 
        }

        public static string FormattedString(this CommandOverload overload)
        {
            var overloadArguments = overload.Arguments.ToList();
            var toString = string.Empty;

            for (int i = 0; i < overloadArguments.Count; i++)
                toString += $"•`{overloadArguments[i].Type.Name} {overloadArguments[i].Name}`, Description: `{(overloadArguments[i].Description == string.Empty || overloadArguments[i].Description == null ? "None" : overloadArguments[i].Description)}`\n";

            return toString == string.Empty ? "No Parameters" : toString;
        }

        public static string Format(this string name)
        {
            var str = name;
            str = str.Remove(0, 1);

            return str.Insert(0, char.ToUpper(name[0]).ToString());
        }
    }
}
