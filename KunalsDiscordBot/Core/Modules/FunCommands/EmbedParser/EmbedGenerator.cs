using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components;
using KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser.Components.DiscordFields;

namespace KunalsDiscordBot.Core.Modules.FunCommands.EmbedParser
{
    public class EmbedGenerator
    {
        public string Source { get; set; }

        public EmbedGenerator()
        {
            
        }

        public EmbedGenerator(string source)
        {
            Source = source;
        }

        public async Task<DiscordEmbedBuilder> Convert()
        {
            var embed = new DiscordEmbedBuilder();
            var components = await ParseComponents(Source);

            components.ForEach(x => x.Modify(embed));
            return embed;
        }

        public async Task<List<EmbedComponent>> ParseComponents(string value)
        {
            bool isFound = false;
            string currentId = string.Empty, currentValue = string.Empty;

            EmbedComponent current = null;
            var components = new List<EmbedComponent>();

            for (int i = 0; i < value.Length; i++)
            {
                switch (value[i])
                {
                    case var x when char.IsWhiteSpace(x) && current == null:
                        break;
                    case '<':
                        isFound = true;
                        break;
                    case '>':
                        if (current == null)
                        {
                            switch (currentId)
                            {
                                case "title":
                                    if (components.Find(x => x.Id == currentId) != null)
                                    {
                                        //error
                                        break;
                                    }

                                    current = new Title();
                                    break;
                                case "description":
                                    if (components.Find(x => x.Id == currentId) != null)
                                    {
                                        //error
                                        break;
                                    }

                                    current = new Description();
                                    break;
                                case "color":
                                    if (components.Find(x => x.Id == currentId) != null)
                                    {
                                        //error
                                        break;
                                    }

                                    current = new Color();
                                    break;
                                case "fields":
                                    if (components.Find(x => x.Id == currentId) != null)
                                    {
                                        //error
                                        break;
                                    }

                                    current = new Fields();
                                    break;
                                case "field":
                                    current = new Field();
                                    break;
                                case "name":
                                    current = new Name();
                                    break;
                                case "value":
                                    current = new Value();
                                    break;
                                case "inline":
                                    current = new Inline();
                                    break;
                            }

                            if (current != null)
                                components.Add(current);
                        }
                        else
                        {
                            if (currentId[0] == '/' && current.Id == currentId.Substring(1))
                            {
                                current.WithInput(currentValue);

                                if (!await current.MatchAndExtract())
                                {
                                    //error
                                    break;
                                }

                                currentValue = string.Empty;
                                current = null;
                            }
                            else
                            {
                                currentValue += $"<{currentId}>";
                            }
                        }

                        currentId = string.Empty;
                        isFound = false;
                        break;
                    default:
                        if (isFound)
                            currentId += value[i];
                        else if (current != null)
                            currentValue += value[i];
                        break;
                }
            }

            return components;
        }
    }
}
