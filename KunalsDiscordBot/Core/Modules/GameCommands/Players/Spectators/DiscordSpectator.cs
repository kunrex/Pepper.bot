﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using KunalsDiscordBot.Modules.Games.Communicators;

namespace KunalsDiscordBot.Modules.Games.Players.Spectators
{
    public class DiscordSpectator : DiscordPlayer<SpectatorCommunicator>
    {
        public DiscordSpectator(DiscordMember _member, DiscordClient _client, ISpectatorGame _game) : base(_member)
        {
            member = _member;
            client = _client;
            game = _game;
        }

        private DiscordClient client { get; set; }
        private ISpectatorGame game { get; set; }

        private bool isEnded = false;

        public void End() => isEnded = true;

        public override Task<bool> Ready(DiscordChannel channel)
        {
            communicator = new SpectatorCommunicator(channel);
            WaitForLeaveMessage();

            return Task.FromResult(true);
        }

        public async Task SendMessage(DiscordEmbedBuilder embed) => await communicator.SendMessage(embed).ConfigureAwait(false);
        public async Task SendMessage(string message) => await communicator.SendMessage(message).ConfigureAwait(false);
        public async Task SendMessage(DiscordMessageBuilder message) => await message.SendAsync(communicator.channel);
        public Task SendMessage(List<Page> pages, PaginationEmojis emojis)
        {
            Task.Run(async () => await communicator.SendPageinatedMessage(member, pages, emojis, PaginationBehaviour.WrapAround, PaginationDeletion.DeleteMessage));

            return Task.CompletedTask;
        }

        private async void WaitForLeaveMessage()
        {
            var interactivity = client.GetInteractivity();

            var message = await interactivity.WaitForMessageAsync(x => x.Channel == communicator.channel && x.Content.ToLower() == "leave" && x.Author.Id == member.Id, TimeSpan.FromHours(1));

            if (isEnded)
                return;

            isEnded = true;
            await communicator.SendMessage("Leaving specator");
            await game.RemoveSpectator(this);
        }
    }
}
