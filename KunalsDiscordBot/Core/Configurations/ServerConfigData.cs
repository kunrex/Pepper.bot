using System;
using System.Collections.Generic;
using KunalsDiscordBot.Services;
using System.Reflection;
using KunalsDiscordBot.Core.Attributes;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace KunalsDiscordBot.Core.Configurations
{
    public class ServerConfigData
    {
        public Dictionary<ConfigValueSet, List<ConfigDataSet>> Values { get; set; }

        public void GatherEditCommands()
        {
            foreach(var module in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsSubclassOf(typeof(BaseCommandModule))))
            {
                var attribute = module.GetCustomAttribute<ConfigDataAttribute>();
                if (attribute == null)
                    continue;

                var name = module.GetCustomAttribute<GroupAttribute>().Name;
                var values = Values[attribute.set];
                foreach(var command in module.GetMethods())
                {
                    attribute = command.GetCustomAttribute<ConfigDataAttribute>();
                    if (attribute == null)
                        continue;

                    var commandName = $"{name} {command.GetCustomAttribute<CommandAttribute>().Name}";

                    for(int i = 0;i<values.Count;i++)
                        if(values[i].ConfigData == attribute.data)
                        {
                            if (values[i].EditCommand != string.Empty && values[i].EditCommand != null)
                                values[i] = new ConfigDataSet
                                {
                                    ConfigData = values[i].ConfigData,
                                    Description = values[i].Description,
                                    FieldName = values[i].FieldName,
                                    EditCommand = $"{values[i].EditCommand}, `{commandName}`"
                                };
                            else
                                values[i] = new ConfigDataSet
                                {
                                    ConfigData = values[i].ConfigData,
                                    Description = values[i].Description,
                                    FieldName = values[i].FieldName,
                                    EditCommand = $"`{commandName}`"
                                };
                        } 
                }
            }
        }
    }
}
