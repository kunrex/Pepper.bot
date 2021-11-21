using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Modules.FunCommands;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{ 
    public class RockPaperScissors : DiscordGame<RockPaperScissorsPlayer, RockPaperScissorsCommunicator>
    {
        private enum RPS
        {
            Rock,
            Paper,
            Scissors,
            None
        }

        private readonly ulong replyMessageId;
        private readonly DiscordChannel channel;
        private DiscordMessage GameMessage { get; set; }

        private RPS player1Choice;
        private RPS player2Choice;

        public RockPaperScissors(DiscordClient _client, List<DiscordMember> _players, DiscordChannel _channel, ulong messageId) : base(_client, _players)
        {
            replyMessageId = messageId;
            channel = _channel;

            Players = _players.Select(x => new RockPaperScissorsPlayer(x)).ToList();
            player1Choice = player2Choice = RPS.None;

            SetUp();
        }

        protected async override void PlayGame()
        {
            var input1 = Task.Run(() => PlayerInput(0));
            var input2 = Task.Run(() => PlayerInput(1));

            await input1;
            await input2;

            string result = "its a tie";
            switch(player1Choice)
            {
                case RPS.Rock:
                    if (player2Choice == RPS.Paper)
                        result = $"{Players[1].member.Mention} wins";
                    else if (player2Choice == RPS.Scissors || player2Choice == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
                case RPS.Paper:
                    if (player2Choice == RPS.Scissors)
                        result = $"{Players[1].member.DisplayName} wins";
                    else if (player2Choice == RPS.Rock || player2Choice == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
                case RPS.Scissors:
                    if (player2Choice == RPS.Rock)
                        result = $"{Players[1].member.DisplayName} wins";
                    else if (player2Choice == RPS.Paper || player2Choice == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
            }

            await PrintFinalBoard(result);
        }

        private async Task PlayerInput(int index)
        {
            var player = Players[index];

            var input = await player.Input(Client, GameMessage);

            if (!input.WasCompleted)
                return;   

            if (index == 0)
                player1Choice = (RPS)input.Ordinate.x;
            else
                player2Choice = (RPS)input.Ordinate.x;

            await PrintBoard();
        }

        protected async Task PrintFinalBoard(string result) => GameMessage = await Players[0].communicator.EditMessage(GameMessage, new DiscordMessageBuilder().WithContent(result)
            .WithEmbed(new DiscordEmbedBuilder
            {
                Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
                Color = DiscordColor.Blurple
            }.AddField($"{Players[0].member.DisplayName}'s choice", player1Choice == RPS.None ? "Didn't chose" : player1Choice.ToString(), true)
             .AddField("** **", "** **", true)
             .AddField($"{Players[1].member.DisplayName}'s choice", player2Choice == RPS.None ? "Didn't chose" : player2Choice.ToString(), true)));

        protected async override Task PrintBoard() => GameMessage = await Players[0].communicator.EditMessage(GameMessage, new DiscordEmbedBuilder
        {
            Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
            Color = DiscordColor.Blurple
        }.AddField($"{Players[0].member.DisplayName}'s choice", player1Choice == RPS.None ? "*Choosing...*" : "Chosen", true)
         .AddField("** **", "** **", true)
         .AddField($"{Players[1].member.DisplayName}'s choice", player2Choice == RPS.None ? "*Choosing...*" : "Chosen", true));

        protected async override void SetUp()
        {
            GameMessage = await new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder
                {
                    Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
                    Color = DiscordColor.Blurple
                }.AddField($"{Players[0].member.DisplayName}'s choice", "*Choosing...*", true)
                 .AddField("** **", "** **", true)
                 .AddField($"{Players[1].member.DisplayName}'s choice", "*Choosing...*", true))
                .WithReply(replyMessageId)
                .AddComponents(new DiscordButtonComponent(ButtonStyle.Primary, "0", "Rock", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":rock:"))),
                               new DiscordButtonComponent(ButtonStyle.Primary, "1", "Papers", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":roll_of_paper:"))),
                               new DiscordButtonComponent(ButtonStyle.Primary, "2", "Scissors", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":scissors:")))).SendAsync(channel);

            await Task.WhenAll(Players.Select(x => Task.Run(() => x.Ready(channel))));         
            PlayGame();
        }
    }
}
