using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace KunalsDiscordBot.Core.Modules.MathCommands.Calculator
{
    public sealed class Calculator
    {
        public ulong ReplyId { get; private set; }

        public DiscordMember Member { get; private set; }
        public DiscordClient Client { get; private set; }
        public DiscordChannel Channel { get; private set; }

        private Page Page { get; }
        private DiscordColor EmbedColor { get; }

        private bool isStopped = false;

        private const string stop = "stop", pi = "pi";

        private const string squared = "squared", toThePower = "power";
        private const string squareRoot = "squareRoot", root = "root";

        private const string sin = "sin", cos = "cos", tan = "tan";

        private DiscordMessage current;
        private string currentStatement;

        public Calculator(ulong _replyId, DiscordChannel _channel, DiscordClient _client, DiscordMember _member, DiscordColor _color)
        {
            ReplyId = _replyId;
            Channel = _channel;

            Client = _client;
            Member = _member;

            EmbedColor = _color;

            Page = new Page()
                .AddRow(
                    new DiscordButtonComponent(ButtonStyle.Primary, sin, sin),
                    new DiscordButtonComponent(ButtonStyle.Primary, cos, cos),
                    new DiscordButtonComponent(ButtonStyle.Primary, tan, tan),
                    new DiscordButtonComponent(ButtonStyle.Danger, stop, "Exit"),
                    new DiscordButtonComponent(ButtonStyle.Success, "=", "=")
                )
                .AddRow(
                    new DiscordButtonComponent(ButtonStyle.Primary, squared, "x²"),
                    new DiscordButtonComponent(ButtonStyle.Primary, toThePower, "x^y"),
                    new DiscordButtonComponent(ButtonStyle.Primary, squareRoot, "²√x"),
                    new DiscordButtonComponent(ButtonStyle.Primary, root, "y√x"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, pi, "π")
                )
                .AddRow(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "7", "7"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "8", "8"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "9", "9"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "+", "+"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "-", "-")
                )
                .AddRow(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "4", "4"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "5", "5"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "6", "6"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "x", "x"),
                    new DiscordButtonComponent(ButtonStyle.Primary, "/", "/")
                )
                .AddRow(
                    new DiscordButtonComponent(ButtonStyle.Secondary, "0", "0"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "1", "1"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "2", "2"),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "3", "3"),
                    new DiscordButtonComponent(ButtonStyle.Primary, ".", ".")
                );

            currentStatement = "0";
            Task.Run(() => InitialPrint());
        }

        private async Task InitialPrint()
        {
            var builder = new DiscordMessageBuilder()
                .WithReply(ReplyId)
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(EmbedColor)
                    .AddField("Statement:", $"```\n{currentStatement}```"));

            foreach (var page in Page.Rows)
                builder.AddComponents(page);

            current = await builder.SendAsync(Channel);

            await Calculate();
        }

        private async Task Print()
        {
            var builder = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(EmbedColor)
                    .AddField("Statement:", $"```\n{currentStatement}```"));

            foreach (var page in Page.Rows)
                builder.AddComponents(page);

            current = await current.ModifyAsync(builder);
        }

        private async Task Calculate()
        {
            var limit = TimeSpan.FromMinutes(1);

            var interactivity = Client.GetInteractivity();

            while(true)
            {
                var buttonResult = await interactivity.WaitForButtonAsync(current, Member, limit);
                await buttonResult.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                if (buttonResult.TimedOut)
                    break;

                switch(buttonResult.Result.Id)
                {
                    case stop:
                        await Stop("Calculator Interaction Ended");
                        return;

                    case sin:
                    case cos:
                    case tan:
                        await TrignometricIdentity(buttonResult.Result.Id);
                        break;
                    case pi:
                        currentStatement += "3.14";
                        break;
                    case "1":
                        currentStatement += "1";
                        break;
                    case "2":
                        currentStatement += "2";
                        break;
                    case "3":
                        currentStatement += "3";
                        break;
                    case "4":
                        currentStatement += "4";
                        break;
                    case "5":
                        currentStatement += "5";
                        break;
                    case "6":
                        currentStatement += "6";
                        break;
                    case "7":
                        currentStatement += "7";
                        break;
                    case "8":
                        currentStatement += "8";
                        break;
                    case "9":
                        currentStatement += "9";
                        break;
                    case "0":
                        currentStatement += "0";
                        break;

                    case ".":
                        currentStatement += ".";
                        break;

                    case "+":
                        currentStatement += "+";
                        break;
                    case "-":
                        currentStatement += "-";
                        break;
                    case "x":
                        currentStatement += "*";
                        break;
                    case "/":
                        currentStatement += "/";
                        break;

                    case "=":
                        currentStatement = $"{await CalculateCurrent()}";
                        break;
                }

                if (isStopped)
                    return;

                await Print();
            }

            await Stop("Interaction Time Out");
        }

        private async Task<float> CalculateCurrent()
        {
            try
            {
                return float.Parse(await new Evaluator(currentStatement).Solve());
            }
            catch(Exception e)
            {
                await Stop(e.Message);
                return 0;
            }
        }

        private async Task Stop(string message)
        {
            isStopped = true;

            var builder = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(EmbedColor)
                    .AddField("Statement:", $"```\n{message}```"));

            current = await current.ModifyAsync(builder);
        }

        private async Task TrignometricIdentity(string identity)
        {
            var result = await CalculateCurrent();

            if (isStopped)
                return;

            switch(identity)
            {
                case sin:
                    currentStatement = $"{Math.Sin(result)}";
                    break;
                case cos:
                    currentStatement = $"{Math.Cos(result)}";
                    break;
                case tan:
                    currentStatement = $"{Math.Tan(result)}";
                    break;
            }
        }
    }
}
