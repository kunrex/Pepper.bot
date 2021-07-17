﻿//System name spaces
using System.Threading.Tasks;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

//Custom name spcaes
using KunalsDiscordBot.Attributes;
using KunalsDiscordBot.Services.Math;

namespace KunalsDiscordBot.Modules.Math
{
    [Group("Math")]
    [DecorAttribute("MidnightBlue", ":1234:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class MathCommands : BaseCommandModule
    {
        [Command("Solve")]
        [Description("Solves an equation")]
        public async Task Solve(CommandContext ctx, [RemainingText] string equation)
        {
            var solver = new LinearEquationSolver(equation);

            string answer = solver.Solve(solver.GetPolynomials(equation));
            await ctx.RespondAsync(answer).ConfigureAwait(false);
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
    }
}
