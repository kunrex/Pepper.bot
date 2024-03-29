﻿using System;
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

        private RPS[] Choices { get; set; }
        private DiscordMessage GameMessage { get; set; }

        private readonly DiscordButtonComponent[] options;

        public RockPaperScissors(DiscordClient _client, List<DiscordMember> _players, DiscordChannel _channel, ulong messageId) : base(_client, _players)
        {
            replyMessageId = messageId;
            channel = _channel;

            Players = _players.Select(x => new RockPaperScissorsPlayer(x)).ToList();
            Choices = new [] { RPS.None, RPS.None };

            options = new DiscordButtonComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, "0", "Rock", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":rock:"))),
                new DiscordButtonComponent(ButtonStyle.Primary, "1", "Paper", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":roll_of_paper:"))),
                new DiscordButtonComponent(ButtonStyle.Primary, "2", "Scissors", false, new DiscordComponentEmoji(DiscordEmoji.FromName(Client, ":scissors:")))
            };
                
            SetUp();
        }

        protected async override void PlayGame()
        {
            for(int i = 0; i < 2; i++)
            {
                CurrentPlayer = Players[i];

                await PrintBoard();
                var input = await CurrentPlayer.Input(Client, GameMessage);

                if(input.WasCompleted)
                    Choices[i] = (RPS)input.Ordinate.x;
            }

            string result = "Its a tie";
            switch(Choices[0])
            {
                case RPS.Rock:
                    if (Choices[1] == RPS.Paper)
                        result = $"{Players[1].member.Mention} wins";
                    else if (Choices[1] == RPS.Scissors || Choices[1] == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
                case RPS.Paper:
                    if (Choices[1] == RPS.Scissors)
                        result = $"{Players[1].member.DisplayName} wins";
                    else if (Choices[1] == RPS.Rock || Choices[1] == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
                case RPS.Scissors:
                    if (Choices[1] == RPS.Rock)
                        result = $"{Players[1].member.DisplayName} wins";
                    else if (Choices[1] == RPS.Paper || Choices[1] == RPS.None)
                        result = $"{Players[0].member.Mention} wins";
                    break;
                case RPS.None:
                    if (Choices[1] == RPS.None)
                        break;
                    else 
                        result = $"{Players[1].member.Mention} wins";
                    break;
            }

            await PrintFinalBoard(result);
        }

        protected async Task PrintFinalBoard(string result) => GameMessage = await Players[0].communicator.EditMessage(GameMessage, new DiscordMessageBuilder().WithContent(result)
            .WithEmbed(new DiscordEmbedBuilder
            {
                Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
                Color = DiscordColor.Blurple
            }.AddField($"{Players[0].member.DisplayName}'s choice", Choices[0] == RPS.None ? "Didn't chose" : Choices[0].ToString(), true)
             .AddField("** **", "** **", true)
             .AddField($"{Players[1].member.DisplayName}'s choice", Choices[1] == RPS.None ? "Didn't chose" : Choices[1].ToString(), true)));

        protected async override Task PrintBoard()
        {
            GameMessage = await Players[0].communicator.EditMessage(GameMessage, new DiscordMessageBuilder().WithContent($"{CurrentPlayer.member.Mention} choose your option")
                .WithEmbed(new DiscordEmbedBuilder
                {
                    Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
                    Color = DiscordColor.Blurple
                }.AddField($"{Players[0].member.DisplayName}'s choice", Choices[0] == RPS.None ? "*Choosing...*" : "Chosen", true)
                 .AddField("** **", "** **", true)
                 .AddField($"{Players[1].member.DisplayName}'s choice", "*Choosing...*", true))
                .AddComponents(options));
        }

        protected async override void SetUp()
        {         
            GameMessage = await new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder
                {
                    Title = $"{Players[0].member.DisplayName} vs {Players[1].member.DisplayName}",
                    Color = DiscordColor.Blurple,
                    Description = "Starting..."
                }).WithReply(replyMessageId).SendAsync(channel);

            await Task.WhenAll(Players.Select(x => Task.Run(() => x.Ready(channel))));         
            PlayGame();
        }
    }
}
