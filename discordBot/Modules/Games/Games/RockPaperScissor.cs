using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace KunalsDiscordBot.Modules.Games
{
    public class RockPaperScissor : Game
    {
        Func<int> GenerateRandom = () => new Random().Next(0, 3);

        public RockPaperScissor(int userChoice, CommandContext ctx)
        {
            int AiChoice = GenerateRandom();
            string message = $"{ctx.User.Mention} chose {EvaluateChoice(userChoice)}, AI chose {EvaluateChoice(AiChoice)}, {Evaluate(userChoice, AiChoice, ctx.Member.Username, "AI")}";

            SendMessage(ctx, message);
        }

        public RockPaperScissor(CommandContext ctx, int player1Choice, int player2Choice, DiscordMember player1, DiscordMember player2)
        {
            string message = $"{player1.Mention} chose {EvaluateChoice(player1Choice)}, {player2.Mention} chose {EvaluateChoice(player2Choice)}, {Evaluate(player1Choice, player2Choice, player1.Username, player2.Username)}";

            SendMessage(ctx, message);
        }

        private async void SendMessage(CommandContext ctx, string message) => await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);

        private string Evaluate(int player1, int player2, string player1Name, string player2Name)
        {
            if (player2 == player1)
                return "Draw";

            switch(player1)
            {
                case 0:
                    if (player2  == 1)
                        return $"{player2Name} wins!";
                    else if (player2 == 2)
                        return $"{player1Name} wins!";
                    break;
                case 1:
                    if (player2 == 2)
                        return $"{player2Name} wins!";
                    else if (player2 == 0)
                        return $"{player1Name} wins!";
                    break;
                case 2:
                    if (player2 == 0)
                        return $"{player2Name} wins!";
                    else if (player2 == 1)
                        return $"{player1Name} wins!";
                    break;
            }

            return string.Empty;
        }

        private string EvaluateChoice(int choice)
        {
            switch(choice)
            {
                case 0:
                    return "rock";
                case 1:
                    return "paper";
                case 2:
                    return "scissors";
            }

            return string.Empty;
        }
    }
}
