using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.ArgumentConverters
{
    public class TimeSpanArgumentConverter : IArgumentConverter<TimeSpan>
    {
        string regexString = "([0-9])+([A-z])\\w";

        public Task<Optional<TimeSpan>> ConvertAsync(string value, CommandContext ctx)
        {
            Console.WriteLine("hello");

            if (TimeSpan.TryParse(value, out var time))
                return Task.FromResult(Optional.FromValue(TimeSpan.Parse(value)));
            else if(value.Length < 3)
            {
                ctx.Channel.SendMessageAsync("Invalid value for a time span, defaulting to 1 day").ConfigureAwait(false);
                return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(1)));
            }

            var regexp = new System.Text.RegularExpressions.Regex(regexString);
            if (!regexp.IsMatch(value))
            {
                ctx.Channel.SendMessageAsync("Invalid value for a time span, defaulting to 1 day").ConfigureAwait(false);
                return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(1)));
            }

            string unit = value.Substring(value.Length - 2);
            string magnitude = value.Substring(0, value.Length - 2);

            if (!int.TryParse(magnitude, out var val))
            {
                ctx.Channel.SendMessageAsync("Invalid value for a time span, defaulting to 1 day").ConfigureAwait(false);
                return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(1)));
            }

            int magnitudeToInt = int.Parse(magnitude);
            switch(unit)
            {
                case "sc":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromSeconds(magnitudeToInt)));
                case "mi":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromMinutes(magnitudeToInt)));
                case "hr":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromHours(magnitudeToInt)));
                case "dy":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt)));
                case "wk":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 7)));
                case "mo":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 30)));
                case "yr":
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(magnitudeToInt * 365)));
                default:
                    ctx.Channel.SendMessageAsync("Invalid value for a time span, defaulting to 1 day");
                    return Task.FromResult(Optional.FromValue(TimeSpan.FromDays(1)));
            }
        }
    }
}
