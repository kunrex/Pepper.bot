//System name spaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

//Custom name spcaes
using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services;
using KunalsDiscordBot.Services.Math;

namespace KunalsDiscordBot.Modules.Math
{
    [Group("Math")]
    [Decor("MidnightBlue", ":1234:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MathCommands : BaseCommandModule
    {
        private static readonly DiscordColor Color = typeof(MathCommands).GetCustomAttribute<DecorAttribute>().color;

        [Command("Solve")]
        [Description("Solves an equation")]
        public async Task Solve(CommandContext ctx, [RemainingText] string equation)
        {
            var solver = new LinearEquationSolver(equation);

            string answer = solver.Solve(solver.GetPolynomials(equation));
            await ctx.RespondAsync(answer).ConfigureAwait(false);
        }

        [Command("Graph")]
        [Description("Graph an equation. Don't use spaces in the equation but spread out different attributes with spaces. API used: https://denzven.pythonanywhere.com/")]
        public async Task Graph(CommandContext ctx, string equation, params string[] attributes)
        {
            string url = $"http://denzven.pythonanywhere.com/DenzGraphingApi/v1/flat_graph/test/plot?formula={Uri.EscapeDataString(equation)}";

            foreach (var attribute in attributes)
                url += $"&{attribute}";

            using (WebClient wc = new WebClient())
            {
                var json = wc.DownloadString(url);

                if (json[0] == '{')//error
                {
                    var jsonData = JsonSerializer.Deserialize<JsonData>(json);
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Graph",
                        Color = Color
                    }.AddField("Error", jsonData.error)
                     .AddField("Error Id", jsonData.error_id)
                     .AddField("Fix", jsonData.fix)).ConfigureAwait(false);
                }
                else
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
                    {
                        Title = "Graph",
                        ImageUrl = url,
                        Footer = BotService.GetEmbedFooter($"Rendered by: {ctx.Member.DisplayName}"),
                        Color = Color
                    });
            }
        }

        [Command("GraphAttributes")]
        [Aliases("Attributes", "atrrs")]
        [Description("Shows a list of attributes you can use for the graph command")]
        public async Task Attributes(CommandContext ctx)
        {
            Dictionary<string, string> attributes = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(Path.Combine("Modules", "Math", "Attributes.json")));

            var embed = new DiscordEmbedBuilder
            {
                Title = "Graph Attributes",
                Description = "All the attributes you can use in the graph command\n",
                Color = Color,
                Footer = BotService.GetEmbedFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")
            };

            foreach (var val in attributes)
                embed.Description += $"• `{val.Key}`: {val.Value}\n";

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("add")]
        [Description("Adds the given 2 numbers")]
        public async Task Add(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 + number2).ToString()).ConfigureAwait(false);

        [Command("subtract")]
        [Description("Subtracts the given 2 numbers")]
        public async Task Sub(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 - number2).ToString()).ConfigureAwait(false);

        [Command("multiply")]
        [Description("Multiplies the given 2 numbers")]
        public async Task Mul(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 * number2).ToString()).ConfigureAwait(false);

        [Command("divide")]
        [Description("Divides the given 2 numbers")]
        public async Task Div(CommandContext ctx, float number1, float number2) => await ctx.RespondAsync((number1 / number2).ToString()).ConfigureAwait(false);

        [Command("sin")]
        [Description("Returns the sin of a number")]
        public async Task Sin(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Sin(number).ToString()).ConfigureAwait(false);

        [Command("cos")]
        [Description("Returns the cos of a number")]
        public async Task Cos(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Cos(number).ToString()).ConfigureAwait(false);

        [Command("tan")]
        [Description("Returns the tan of a number")]
        public async Task Tan(CommandContext ctx, float number) => await ctx.RespondAsync(System.Math.Tan(number).ToString()).ConfigureAwait(false);

        [Command("cosecant")]
        [Aliases("cosec")]
        [Description("Returns the cosecant of a number")]
        public async Task Cosecant(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Sin(number)).ToString()).ConfigureAwait(false);

        [Command("secant")]
        [Aliases("sec")]
        [Description("Returns the cosecant of a number")]
        public async Task Secant(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Cos(number)).ToString()).ConfigureAwait(false);

        [Command("cotangent")]
        [Aliases("cot")]
        [Description("Returns the cosecant of a number")]
        public async Task Cotangent(CommandContext ctx, float number) => await ctx.RespondAsync((1f / System.Math.Tan(number)).ToString()).ConfigureAwait(false);

        [Command("pi")]
        [Description("value of PI")]
        public async Task Pi(CommandContext ctx) => await ctx.RespondAsync(System.Math.PI.ToString()).ConfigureAwait(false);

        [Command("e")]
        [Description("value of E")]
        public async Task E(CommandContext ctx) => await ctx.RespondAsync(System.Math.E.ToString()).ConfigureAwait(false);

        [Command("squareroot")]
        [Aliases("sqrt")]
        [Description("Returns the square root of a number")]
        public async Task Sqrt(CommandContext ctx, float number) => await ctx.RespondAsync((System.Math.Sqrt(number)).ToString()).ConfigureAwait(false);

        [Command("root")]
        [Description("Returns the N'th root of a number")]
        public async Task Root(CommandContext ctx, float number, float root) => await ctx.RespondAsync((System.Math.Pow(number, 1.0 / root)).ToString()).ConfigureAwait(false);

        [Command("pow")]
        [Description("Returns the N'th power of a number")]
        public async Task Pow(CommandContext ctx, float number, float root) => await ctx.RespondAsync((System.Math.Pow(number, root)).ToString()).ConfigureAwait(false);

        [Command("log")]
        [Description("Returns the base 10 logarithm of a number")]
        public async Task Log(CommandContext ctx, float number) => await ctx.RespondAsync((System.Math.Log10(number)).ToString()).ConfigureAwait(false);

        private class JsonData
        {
            public string error { get; set; }
            public string error_id { get; set; }
            public string fix { get; set; }
        }
    }
}
