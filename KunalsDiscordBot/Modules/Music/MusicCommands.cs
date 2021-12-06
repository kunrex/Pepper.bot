using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity.EventHandling;

using KunalsDiscordBot.Services;
using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Services.Music;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Core.DiscordModels;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Attributes.MusicCommands;
using KunalsDiscordBot.Core.Configurations.Attributes;
using KunalsDiscordBot.Core.DialogueHandlers.Steps.Basics;

namespace KunalsDiscordBot.Modules.Music
{
    [Group("Music")]
    [Decor("Aquamarine", ":musical_note:")]
    [Description("Commands to play music on discord."), ConfigData(ConfigValueSet.Music)]
    [ModuleLifespan(ModuleLifespan.Transient), RequireBotPermissions(Permissions.SendMessages | Permissions.AccessChannels | Permissions.UseVoice | Permissions.Speak)]
    public sealed class MusicCommands : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IMusicService service;
        private readonly IServerService serverService;
        private readonly IPlaylistService playlistService;

        public MusicCommands(IMusicService _service, IServerService _serverService, IModuleService moduleService, IPlaylistService _playlistService)
        {
            service = _service;
            serverService = _serverService;
            playlistService = _playlistService;
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.Music];
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var myInstance = PepperBotClientManager.GetShard(ctx.Client.ShardId);
            var requireLavalink = ctx.Command.CustomAttributes.FirstOrDefault(x => x is NonLavalinkCommandAttribute) != null;

            var lava = ctx.Client.GetLavalink();
            if ((!myInstance.IsOnline || lava == null || !lava.ConnectedNodes.Any()) && !requireLavalink)
            {
                await ctx.Channel.SendMessageAsync("The LavaLink connection has not been established, cannot play music without an established connection");
                throw new CustomCommandException();
            }

            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigigurationPermissionsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & Permissions.Administrator) != Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }

            var botVCCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is BotVCNeededAttribute) != null;
            var playerChannel = await service.GetPlayerChannel(ctx.Guild.Id);
            if (botVCCheck && playerChannel == null)
            {
                await ctx.RespondAsync("I'm not in a voice channel?");
                throw new CustomCommandException();
            }

            var userVCCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is UserVCNeededAttribute) != null;
            if (userVCCheck)
            {
                var voiceState = ctx.Member.VoiceState;

                if (voiceState == null || voiceState.Channel == null || voiceState.Channel.Type != ChannelType.Voice)
                {
                    await ctx.RespondAsync("You need to be in a Voice Channel to run this command");
                    throw new CustomCommandException();
                }
                else if (voiceState.IsSelfDeafened || voiceState.IsServerDeafened)
                {
                    await ctx.RespondAsync("You can't run this command while deafened");
                    throw new CustomCommandException();
                }
                else if (botVCCheck && voiceState.Channel.Id != playerChannel.Id)
                {
                    await ctx.RespondAsync("You need to be in the same Voice Channel as Pepper to run this command");
                    throw new CustomCommandException();
                }
            }

            var musicData = await serverService.GetMusicData(ctx.Guild.Id);
            var DJCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is DJCheckAttribute) != null && musicData.UseDJRoleEnforcement == 1;

            if (DJCheck)
            {
                if (userVCCheck && ctx.Member.VoiceState.Channel.Users.ToList().Count == 2)
                    return;

                var id = (ulong)musicData.DJRoleId;
                if (id == 0)
                    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription("No DJ role is stored for the server, all users will be able to run DJ commands").WithColor(ModuleInfo.Color));
                else
                {
                    var role = ctx.Guild.GetRole(id);

                    if (ctx.Member.Roles.FirstOrDefault(x => x.Id == role.Id || x.Position > role.Position) == null)
                    {
                        await ctx.RespondAsync(":x: You need the DJ role to run this command");
                        throw new CustomCommandException();
                    }
                }
            }

            await base.BeforeExecutionAsync(ctx);
        }

        [Command("ToggleDJ")]
        [Aliases("DJ")]
        [Description("Changes wether or not the DJ role should be enforced for music commands")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.DJEnfore)]
        public async Task ToggeDJ(CommandContext ctx, bool toChange)
        {
            await serverService.ModifyData(await serverService.GetMusicData(ctx.Guild.Id), x => x.UseDJRoleEnforcement = toChange ? 1 : 0).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `Enforce DJ Role` to {toChange}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName} at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("DJRole")]
        [Description("Assigns the DJ role for a server")]
        [CheckConfigigurationPermissions, ConfigData(ConfigValue.DJRole)]
        public async Task DJRole(CommandContext ctx, DiscordRole role)
        {
            var profile = await serverService.GetMusicData(ctx.Guild.Id).ConfigureAwait(false);
            if (profile.UseDJRoleEnforcement == 0)
            {
                await ctx.RespondAsync("`Enforce DJ Role` must be set to true to use this command, you can do so using the `music toggleDJ` command").ConfigureAwait(false);
                return;
            }

            await serverService.ModifyData(await serverService.GetMusicData(ctx.Guild.Id), x => x.DJRoleId = (long)role.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {role.Mention} as the DJ role for the server",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("Join")]
        [Description("Joins a voice channel")]
        [UserVCNeeded]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (await service.GetPlayerChannel(ctx.Guild.Id) != null)
            {
                await ctx.RespondAsync("I'm already in a channel?").ConfigureAwait(false);
                return;
            }

            var channel = ctx.Member.VoiceState.Channel;
            var node = lava.ConnectedNodes.Values.First();

            var message = await service.CreatePlayer(ctx.Guild.Id, node, lava, channel, ctx.Channel);
            await ctx.Channel.SendMessageAsync(message).ConfigureAwait(false);
        }

        [Command("Leave")]
        [Description("Leaves the joined channel")]
        [UserVCNeeded, BotVCNeeded, DJCheck]
        public async Task Leave(CommandContext ctx) => await ctx.Channel.SendMessageAsync($"{await service.DisconnectPlayer(ctx.Guild.Id)}");

        [Command("Play")]
        [Description("Plays a song")]
        [UserVCNeeded, BotVCNeeded]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            var embed = await service.Play(ctx.Guild.Id, search, ctx.Member.DisplayName, ctx.Member.Id);
            if (embed == null)
                return;

            await ctx.Channel.SendMessageAsync(embed).ConfigureAwait(false);
        }

        [Command("Queue")]
        [Description("Gets the players queue")]
        [BotVCNeeded]
        public async Task GetQueue(CommandContext ctx)
        {
            int index = 1;
            var embeds = await service.GetQueue(ctx.Guild.Id);
            embeds.ForEach(x => x.WithFooter($"Page: {index++}, Requested By {ctx.Member.DisplayName}"));

            var pages = embeds.Select(x => new Page(null, x));
            await ctx.Channel.SendPaginatedMessageAsync(ctx.User, pages, default, PaginationBehaviour.Ignore, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("Pause")]
        [Description("Pauses the player")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Pause(CommandContext ctx) => await ctx.RespondAsync(await service.Pause(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("Resume")]
        [Description("Resumes the player")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Resume(CommandContext ctx) => await ctx.RespondAsync(await service.Resume(ctx.Guild.Id));

        [Command("Loop")]
        [Description("Toggles if the current track should loop or not")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Loop(CommandContext ctx) => await ctx.RespondAsync(await service.Loop(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("QueueLoop")]
        [Aliases("ql")]
        [Description("Toggles if the queue should loop or not, this does not include the track being played")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task QueueLoop(CommandContext ctx) => await ctx.RespondAsync(await service.QueueLoop(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("Remove")]
        [Description("Removes a search from the queue")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Remove(CommandContext ctx, int index) => await ctx.RespondAsync(await service.Remove(ctx.Guild.Id, index)).ConfigureAwait(false);

        [Command("Skip")]
        [Description("Skips the current track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Skip(CommandContext ctx) => await ctx.RespondAsync(await service.Skip(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("NowPlaying")]
        [Aliases("np")]
        [Description("Gets info about the current track"), BotVCNeeded]
        public async Task NowPlaying(CommandContext ctx) => await ctx.RespondAsync((await service.NowPlaying(ctx.Guild.Id)).WithColor(ModuleInfo.Color)).ConfigureAwait(false);

        [Command("Move")]
        [Description("Moves a track around")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Move(CommandContext ctx, int trackToMove, int newPosition) => await ctx.RespondAsync(await service.Move(ctx.Guild.Id, trackToMove, newPosition)).ConfigureAwait(false);

        [Command("Clear")]
        [Description("Clears the track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Clear(CommandContext ctx) => await ctx.RespondAsync(await service.ClearQueue(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("PlayFrom")]
        [Aliases("pf", "seek")]
        [Description("Starts playing from a specified position")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Seek(CommandContext ctx, TimeSpan span) => await ctx.RespondAsync(await service.Seek(ctx.Guild.Id, span)).ConfigureAwait(false);

        [Command("Forward")]
        [Description("Make the track move forward")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Forward(CommandContext ctx, TimeSpan span) => await ctx.RespondAsync(await service.Seek(ctx.Guild.Id, span, true)).ConfigureAwait(false);

        [Command("Rewind")]
        [Description("Rewind the track")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Rewind(CommandContext ctx, TimeSpan span) => await ctx.RespondAsync(await service.Seek(ctx.Guild.Id, span.Negate(), true)).ConfigureAwait(false);

        [Command("Clean")]
        [Description("Cleans the queue. If a track is in the queue and the user who requested is not in the channel, the track is removed")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Clean(CommandContext ctx) => await ctx.RespondAsync(await service.Clean(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("Shuffle")]
        [Description("Shuffle the queue")]
        [DJCheck, UserVCNeeded, BotVCNeeded]
        public async Task Shuffle(CommandContext ctx) => await ctx.RespondAsync(await service.Shuffle(ctx.Guild.Id)).ConfigureAwait(false);

        [Command("CreatePlaylist")]
        [Description("Creates a new playlist"), DJCheck, NonLavalinkCommand, ConfigData(ConfigValue.PlaylistCount)]
        public async Task CreatePlaylist(CommandContext ctx, [RemainingText] string name)
        {
            var playlists = (await playlistService.GetPlaylists(ctx.Guild.Id)).ToArray();
            if (playlists.Length == service.ModuleData.MaxPlayistCount)
            {
                await ctx.RespondAsync("This server already has 3 playlists");
                return;
            }

            var casted = name.ToLower();
            if(playlists.FirstOrDefault(x => x.PlaylistName.ToLower() == casted) != null)
            {
                await ctx.RespondAsync($"This server already has playlist named {name}");
                return;
            }

            await playlistService.CreatePlaylist(ctx.Guild.Id, ctx.Member.Id, name);
            await ctx.RespondAsync($"Playlist: `{name}` created!");
        }

        [Command("GetPlaylists")]
        [Description("Gets all server playlist"), NonLavalinkCommand]
        public async Task GetPlaylists(CommandContext ctx)
        {
            var playlists = await playlistService.GetPlaylists(ctx.Guild.Id);

            if (playlists.Count() == 0)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = "Server has no playlists"
                });

                return;
            }

            var pages = ctx.Client.GetInteractivity().GetPages(playlists, x => (x.PlaylistName, $"Created by: <@{(ulong)x.AuthorId}>"), new EmbedSkeleton
            {
                Color = ModuleInfo.Color,
                Title = $"Playlists in {ctx.Guild.Name}",
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Requested By: {ctx.Member.DisplayName} at {DateTime.Now}"}
            }).ToArray();

            if (pages.Length == 1)
                await ctx.RespondAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("DeletePlaylist")]
        [Description("Creates a new playlist"), DJCheck, NonLavalinkCommand, ConfigData(ConfigValue.PlaylistCount)]
        public async Task DeletePlaylist(CommandContext ctx, [RemainingText] string name)
        {
            await playlistService.DeletePlaylist(ctx.Guild.Id, name);

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Color = ModuleInfo.Color,
                Description = $"Playlist: `{name}` deleted"
            });
        }

        [Command("RenamePlaylist")]
        [Description("Renames a playlist"), DJCheck, NonLavalinkCommand]
        public async Task RenamePlaylist(CommandContext ctx, [RemainingText] string name)
        {
            var split = name.Split(',').Select(x => x.Trim()).ToArray();
            if (split.Length != 2)
            {
                await ctx.RespondAsync("Split the playlist and the new name of the playlist with a `,`");
                return;
            }

            var completed = await playlistService.RenamePlaylist(ctx.Guild.Id, split[0], split[1]);

            if (completed)
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Playlist: `{split[0]}` renamed to `{split[1]}`"
                });        
            else
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Playlist: `{split[0]}` doesn't exist"
                });
        }

        [Command("Playlist")]
        [Description("Gets a playlist"), NonLavalinkCommand]
        public async Task Playlist(CommandContext ctx, [RemainingText] string name)
        {
            var tracks = await playlistService.GetTracks(ctx.Guild.Id, name);
            if (tracks == null)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Playlist: `{name}` doesn't exist"
                });

                return;
            }

            if (tracks.Count() == 0)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Playlist `{name}` has no tracks"
                });

                return;
            }

            var pages = ctx.Client.GetInteractivity().GetPages(tracks, x => ("Track", $"Search: `{x.URI}`, Added by: <@{(ulong)x.AddedById}>"), new EmbedSkeleton
            {
                Color = ModuleInfo.Color,
                Title = $"Tracks in {name}",
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Requested By: {ctx.Member.DisplayName} at {DateTime.Now}" }
            }).ToArray();

            if (pages.Length == 1)
                await ctx.RespondAsync(pages[0].Embed);
            else
                await ctx.Channel.SendPaginatedMessageAsync(ctx.Member, pages, PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable, new CancellationTokenSource(TimeSpan.FromMinutes(1)).Token);
        }

        [Command("AddTrack")]
        [Description("Adds a track to a playlist"), DJCheck, NonLavalinkCommand]
        public async Task AddTrack(CommandContext ctx, [RemainingText] string tracksAndPlaylist)
        {
            var split = tracksAndPlaylist.Split(',').Select(x => x.Trim().ToLower()).Distinct().ToArray();
            if (split.Length < 2)
            {
                await ctx.RespondAsync("Split the tracks and the name of the playlist you want to add the tracks to with `,`'s");
                return;
            }

            var playListName = split[split.Length - 1];
            var playlist = await playlistService.GetPlaylist(ctx.Guild.Id, playListName);

            if (playlist == null)
            {
                await ctx.RespondAsync($"Playlist `{playListName}` does not exist");
                return;
            }

            var tracks = (await playlistService.GetTracks(playlist)).ToArray();
            if (tracks.Length == service.ModuleData.MaxQueueLength)
            {
                await ctx.RespondAsync($"Playlist {playListName} has a max length of {service.ModuleData.MaxQueueLength}");
                return;
            }

            var added = new List<string>();
            for (int i = 0; i < split.Length - 1; i++)
            {
                if(tracks.Length + added.Count == service.ModuleData.MaxQueueLength)
                {
                    await ctx.RespondAsync($"Playlist {playListName} has a max length of {service.ModuleData.MaxQueueLength}");
                    break;
                }

                var completed = await playlistService.AddTrack(playlist, ctx.Member.Id, split[i]);

                if (!completed)
                    await ctx.RespondAsync(new DiscordEmbedBuilder
                    {
                        Color = ModuleInfo.Color,
                        Description = $"Track `{split[i]}` already exists in playlist: `{playListName}`"
                    });
                else
                    added.Add(split[i]);
            }

            await ctx.RespondAsync(new DiscordEmbedBuilder
            {
                Color = ModuleInfo.Color,
                Description = $"Tracks {string.Join(", ", added.Select(x => $"`{x}`"))} added to playlist: `{playListName}`"
            });
        }

        [Command("RemoveTrack")]
        [Description("Removes a track from a playlist"), DJCheck, NonLavalinkCommand]
        public async Task RemoveTrack(CommandContext ctx, int index, [RemainingText] string playlistName)
        {
            var playlist = await playlistService.GetPlaylist(ctx.Guild.Id, playlistName);

            if (playlist == null)
            {
                await ctx.RespondAsync($"Playlist `{playlistName}` does not exist");
                return;
            }

            var completed = await playlistService.RemoveTrack(playlist, index - 1);

            if (completed)
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Track `{index}` removed from playlist: `{playlistName}`"
                });
            else
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Track `{index}` doesn't exist in playlist: `{playlistName}`"
                });
        }

        [Command("QueuePlaylist")]
        [Description("Queues a playlist")]
        [UserVCNeeded, BotVCNeeded]
        public async Task QueuePlaylist(CommandContext ctx, [RemainingText] string playlistName)
        {
            var reply = await new MessageStep("Do you want to shuffle the playlist?", string.Empty, 10)
                .WithMesssageData(new MessageData
                {
                    Reply = true,
                    ReplyId = ctx.Message.Id
                }).ProcessStep(ctx.Channel, ctx.Member, ctx.Client, false);

            bool shuffle = false;
            try
            {
                var converted = await ctx.CommandsNext.ConvertArgument(reply.Result, ctx, typeof(bool));
                shuffle = (bool)converted;
            }
            catch
            {
                await ctx.RespondAsync("Invalid answer, defaulting to false");
            }

            var tracks = (await playlistService.GetTracks(ctx.Guild.Id, playlistName));
            if (tracks == null)
            {
                await ctx.RespondAsync($"Playlist `{playlistName}` does not exist");
                return;
            }

            if (tracks.Count() == 0)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Color = ModuleInfo.Color,
                    Description = $"Playlist `{playlistName}` has no tracks"
                });

                return;
            }

            var playlist = shuffle ? tracks.ToArray().Shuffle() : tracks;
            await ctx.RespondAsync(await service.Play(ctx.Guild.Id, ctx.Member.DisplayName, ctx.Member.Id, playlist.ToArray()));
        }
    }
}
