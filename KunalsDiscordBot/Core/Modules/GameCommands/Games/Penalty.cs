using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.Events;
using KunalsDiscordBot.Core.Modules.GameCommands.Communicators;
using KunalsDiscordBot.Core.Modules.GameCommands.Players;

namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public sealed class Penalty : DiscordGame<PenaltyPlayer, PenaltyCommunicator>
    {
        public static readonly TimeSpan totalTime = TimeSpan.FromMinutes(1);
        private static readonly string goal = ":goal: :goal: :goal:", keeper = ":levitate:", blackBox = ":black_large_square:", football = ":soccer:",
            scored = "https://thumbs.gfycat.com/OccasionalWealthyCuscus-size_restricted.gif", blocked = "https://i.makeagif.com/media/7-21-2015/8QjzoI.gif";
        private static readonly string[] line = new string[] { blackBox, blackBox, blackBox };

        private readonly DiscordChannel channel;
        private readonly ulong ReplyMessageId;

        private DiscordMessage GameMessage { get; set; }
        private int currentColumn { get; set; }

        private readonly CancellationTokenSource source = new CancellationTokenSource();
        private readonly CancellationToken cancellationToken; 

        public Penalty(DiscordClient _client, List<DiscordMember> _players, DiscordChannel _channel, ulong messageId) : base(_client, _players)
        {
            Players = new List<PenaltyPlayer>() { new PenaltyPlayer(_players[0]) };
            CurrentPlayer = Players[0];

            channel = _channel;
            GameOver = false;
            cancellationToken = source.Token;

            ReplyMessageId = messageId;
            OnGameOver = new SimpleBotEvent();

            SetUp();
        }

        protected async override void SetUp()
        {
            await CurrentPlayer.Ready(channel);

            var _line = (string[])line.Clone();
            currentColumn = new Random().Next(0, 3);
            _line[currentColumn] = keeper;

            var message = $"{goal}\n{string.Join(' ', _line)}\n{string.Join(' ', line)}\n{blackBox} {football} {blackBox}";

            GameMessage = await CurrentPlayer.SendMessage(new DiscordMessageBuilder()
               .WithContent(message)
               .WithReply(ReplyMessageId, true)
               .WithEmbed(new DiscordEmbedBuilder()
                .WithDescription($"{CurrentPlayer.member.Mention} Enter the column in which you want to shoot")
                .WithColor(DiscordColor.White)));

            PlayGame();
        }

        protected async override void PlayGame()
        {
            var whenAny = Task.WhenAny(PrintBoard(), Task.Run(async () =>
            {
                while (!GameOver)
                {
                    var input = await CurrentPlayer.Input(Client);

                    if (input.WasCompleted)
                    {
                        source.Cancel();
                        if (input.Ordinate.x != currentColumn && new Random().Next(1, 10) > 5)//last second block
                            currentColumn = input.Ordinate.x;

                        await EndMessage(input.Ordinate.x != currentColumn, input.Ordinate.x);
                        GameOver = true;
                    }
                }
            }));

            await whenAny;

            source.Dispose();
            OnGameOver.Invoke();
        }

        protected async override Task PrintBoard()
        {
            while(true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(new Random().Next(1, 4)), cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    break;

                var _line = (string[])line.Clone();
                var list = Enumerable.Range(0, 3).ToList();
                list.Remove(currentColumn);

                currentColumn = list[new Random().Next(0, list.Count)];
                _line[currentColumn] = keeper;

                var message = $"{goal}\n{string.Join(' ', _line)}\n{string.Join(' ' , line)}\n{blackBox} {football} {blackBox}";

                GameMessage = await CurrentPlayer.EditMessage(GameMessage, new DiscordMessageBuilder()
                    .WithContent(message)
                    .WithEmbed(new DiscordEmbedBuilder()
                        .WithDescription($"{CurrentPlayer.member.Mention} Enter the column in which you want to shoot")
                        .WithColor(DiscordColor.White)));
            }
        }

        private async Task EndMessage(bool won, int columnPlayer)
        {
            var _line = (string[])line.Clone();
            _line[currentColumn] = keeper;
            if (won)
                _line[columnPlayer] = football;

            var _line2 = (string[])line.Clone();
            if (!won)
                _line2[currentColumn] = football;

            var message = $"{goal}\n{string.Join(' ', _line)}\n{string.Join(' ', _line2)}\n{string.Join(' ', line)}";
            var embedDescription = won ? "Good job you scored a goal!" : "Yea you didn't score kek, basically just gave the keeper the ball lmao";

            GameMessage = await CurrentPlayer.EditMessage(GameMessage, new DiscordMessageBuilder()
                .WithEmbed(new DiscordEmbedBuilder()
                    .WithDescription(embedDescription)
                    .WithImageUrl(won ? scored : blocked)
                    .WithColor(DiscordColor.White))
                .WithContent(message));
        }
    }
}
