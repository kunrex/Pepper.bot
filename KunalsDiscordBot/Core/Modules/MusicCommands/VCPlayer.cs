using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using KunalsDiscordBot.Extensions;
using KunalsDiscordBot.Core.Events;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public sealed class VCPlayer 
    {
        private static readonly int Height = 30;
        private static readonly int Width = 40;

        public Queue<QueueData> queue { get; private set; }
        public LavalinkGuildConnection connection { get; private set; }
        public LavalinkExtension lava { get; private set; }
        public LavalinkNodeConnection node { get; private set; }

        public SimpleBotEvent OnDisconnect { get; private set; } = new SimpleBotEvent();

        private readonly MusicModuleData moduleData;

        private LavalinkTrack currentTrack { get => connection.CurrentState.CurrentTrack; }
        private string memberWhoRequested = string.Empty;

        private bool isPaused { get; set; }
        private bool isLooping { get; set; }
        private bool queueLoop { get; set; }
        private bool isConnected { get; set; } = false;

        private DiscordChannel boundChannel { get; set; }
        private CancellationTokenSource inactivityCancellationToken { get; set; }

        public VCPlayer(MusicModuleData _moduleData, LavalinkNodeConnection nodeConnection, LavalinkExtension extension)
        {
            moduleData = _moduleData;
            queue = new Queue<QueueData>();

            node = nodeConnection;
            lava = extension;
        }

        public async Task<string> Connect(DiscordChannel _channel, DiscordChannel _boundChannel)
        {
            if (isConnected)
                return "";

            isConnected = true;

            connection = await node.ConnectAsync(_channel);

            connection.PlaybackFinished += OnSongFinish;
            connection.TrackException += OnTrackError;
            connection.TrackStuck += OnTrackStuck;
            connection.PlaybackStarted += OnSongStart;

            boundChannel = _boundChannel;

            return $"Joined <#{_channel.Id}> and bound to <#{_boundChannel.Id}> \nUse the `play` command to play some music";
        }

        public async Task<string> Disconnect()
        {
            if (!isConnected)
                return "";

            isConnected = false;

            await connection.DisconnectAsync();
            OnDisconnect.Invoke();
            inactivityCancellationToken.Dispose();

            return $"Left {connection.Channel.Mention} succesfully";
        }

        private Task OnSongFinish(LavalinkGuildConnection connect, TrackFinishEventArgs args)
        {
            Task.Run(async () => await PlayNext());

            return Task.CompletedTask;
        }

        private Task OnTrackError(LavalinkGuildConnection connect, TrackExceptionEventArgs args)
        {
            Task.Run(async () =>
            {
                await boundChannel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "An error occured, this may lead to the current track getting skipped.",
                    Color = DiscordColor.Red
                });
            });

            return Task.CompletedTask;
        }

        private Task OnTrackStuck(LavalinkGuildConnection connect, TrackStuckEventArgs args)
        {
            Task.Run(async () =>
            {
                await boundChannel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "Current track got stuck",
                    Color = DiscordColor.Red
                });
             });

            return Task.CompletedTask;
        }

        private Task OnSongStart(LavalinkGuildConnection connect, TrackStartEventArgs args)
        {
            Task.Run(async () => await boundChannel.SendMessageAsync(await NowPlaying()));

            return Task.CompletedTask;
        }

        public async Task<DiscordEmbedBuilder> StartPlaying(string search, string member, ulong id)
        {
            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                return new DiscordEmbedBuilder().WithDescription($"Track search failed for {search}");

            if (queue.Count == moduleData.maxQueueLength)
                return new DiscordEmbedBuilder().WithDescription($"Queue is at max length ({moduleData.maxQueueLength}), remove tracks to add more");

            if (inactivityCancellationToken != null)//inactivity check was started
                inactivityCancellationToken.Cancel();

            if (currentTrack == null)
            {
                var track = loadResult.Tracks.First();
                memberWhoRequested = member;

                await connection.PlayAsync(track);

                return null;//handled by now playing function
            }
            else
            {
                var track = loadResult.Tracks.First();
                queue.Enqueue(new QueueData { userName = member , id = id, track = track });
                return new DiscordEmbedBuilder().WithDescription($"Queued [{track.Title}]({track.Uri}) at index `{queue.Count}`").WithColor(moduleData.color);
            }
        }

        private async Task PlayNext(bool considerLoop = true)
        {
            if (isLooping && considerLoop)
            {
                await connection.PlayAsync(currentTrack);
 
                return;
            }

            if (queue.Count <= 0)
            {
                await boundChannel.SendMessageAsync("Queue Finished");
                inactivityCancellationToken = new CancellationTokenSource();

                await Task.Run(() => InactivityCheck());
                return;
            }

            var search = queue.Dequeue();
            memberWhoRequested = search.userName;

            await connection.PlayAsync(search.track);

            if (queueLoop)//re add the search
                queue.Enqueue(search);
        }

        private async Task InactivityCheck()
        {
            if (inactivityCancellationToken == null || inactivityCancellationToken.IsCancellationRequested)
                return;

            await Task.Delay(TimeSpan.FromMinutes(moduleData.inactivityLength), inactivityCancellationToken.Token);

            if (inactivityCancellationToken.IsCancellationRequested)
            {
                inactivityCancellationToken = null;
                return;
            }

            await boundChannel.SendMessageAsync("Leaving due to inactivity").ConfigureAwait(false);
            await Disconnect();
        }

        public async Task<string> Pause()
        {
            if (isPaused)
                return "Player already paused";

            await connection.PauseAsync();
            isPaused = true;

            return "Player Paused";
        }

        public async Task<string> Resume()
        {
            if (!isPaused)
                return "Player in't paused";

            await connection.ResumeAsync();
            isPaused = false;
            return "Player Resumed";
        }

        public Task<string> Remove(int index)
        {
            if (queue.Count > 0)
            {
                var queueToList = queue.ToList();

                if ((index - 1) >= queueToList.Count || (index - 1) < 0)
                    return Task.FromResult("Index does not exist");

                queueToList.RemoveAt(index - 1);

                var newQueue = new Queue<QueueData>();

                foreach (var value in queueToList)
                    newQueue.Enqueue(value);

                queue = newQueue;
                return Task.FromResult($"Removed at {index}");
            }
            else
                return Task.FromResult("Queue is Empty");
        }

        public Task<bool> Loop() => Task.FromResult(isLooping ^= true);

        public Task<bool> QueueLoop() => Task.FromResult(queueLoop ^= true);

        public Task<List<DiscordEmbedBuilder>> GetQueue()
        {
            List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();
            int index = 0;
            var title = $"Queue For __{connection.Channel.Guild.Name}__";

            embeds.Add(new DiscordEmbedBuilder().WithTitle("Now Playing").WithColor(moduleData.color).WithDescription($"[{currentTrack.Title}]({currentTrack.Uri})"));
            DiscordEmbedBuilder currentEmbed = null;

            foreach(var value in queue)
            {
                if (index % moduleData.queuePageLimit == 0)
                {
                    currentEmbed = new DiscordEmbedBuilder().WithTitle(title).WithColor(moduleData.color);
                    embeds.Add(currentEmbed);
                }

                currentEmbed.Description += $"{++index}. [{value.track.Title}]({value.track.Uri}) \n Length: {value.track.Length:mm\\:ss}, Requested By: `{value.userName}`\n";
            }

            if (embeds.Count == 1)
                embeds.Add(new DiscordEmbedBuilder().WithTitle(title).WithColor(moduleData.color).WithDescription("Queue is empty"));

            return Task.FromResult(embeds);
        }

        public Task<DiscordEmbedBuilder> NowPlaying()
        {
            if (currentTrack == null)
                return Task.FromResult(new DiscordEmbedBuilder
                {
                    Title = $"Now Playing: Nothing",
                    Description = "Use the `play` command to play some music."
                }); 

            string thumbnailURL = $"https://img.youtube.com/vi/{currentTrack.Uri.AbsoluteUri.GetYoutubeThumbailURL()}/default.jpg";

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Now Playing: {currentTrack.Title}",
                Description = $"`Length:` {currentTrack.Length}\n `Author:` {currentTrack.Author}\n",
                Url = currentTrack.Uri.AbsoluteUri
            }.AddField("`Requested By:` ", memberWhoRequested)
             .AddField("`Position`", $"{connection.CurrentState.PlaybackPosition:mm\\:ss}")
             .AddField("`Next Track:` ", queue.TryPeek(out QueueData result) ? $"[{queue.Peek().track.Title}]({queue.Peek().track.Uri})" : "Nothing", true)
             .AddField("`Paused`", isPaused.ToString(), true)
             .AddField("`Looping`", isLooping.ToString(), true)
             .AddField("`Queue Loop`", queueLoop.ToString(), true)
             .WithThumbnail(thumbnailURL, Height, Width)
             .WithColor(moduleData.color);

            return Task.FromResult(embed);
        }

        public Task<string> Move(int trackToMove, int newIndex)
        {
            List<QueueData> tracks = queue.ToList();

            if(queue.Count <= 1)
                return Task.FromResult("Queue only has 1 elemant");
            if (trackToMove < 1 || trackToMove > queue.Count || newIndex == trackToMove || newIndex < 1 || newIndex > queue.Count)
                return Task.FromResult("Invalid position(s), use the `queue` command to view the queue and enter valid position(s)");

            var track = tracks[trackToMove - 1];
            tracks.RemoveAt(trackToMove - 1);

            tracks.Insert(newIndex - 1, track);

            Queue<QueueData> newQueue = new Queue<QueueData>();
            foreach (var _track in tracks)
                newQueue.Enqueue(_track);

            queue = newQueue;

            return Task.FromResult("Moved Track");
        }

        public Task<string> ClearQueue()
        {
            if (queue.Count == 0)
                return Task.FromResult("Queue is already empty");

            queue = new Queue<QueueData>();

            return Task.FromResult("Queue cleared");
        }

        public async Task Skip() => await connection.StopAsync();

        public async Task<string> Seek(TimeSpan span, bool relative = false)
        {
            if (span > TimeSpan.FromSeconds(0) ? (relative ? span + connection.CurrentState.PlaybackPosition > currentTrack.Length  : span > currentTrack.Length) : (relative ? span + connection.CurrentState.PlaybackPosition < TimeSpan.FromSeconds(0) : span < TimeSpan.FromSeconds(0)))
                return "Cannot play from specified position";

            var newSpan = relative ? connection.CurrentState.PlaybackPosition + span : span;
            if (relative)
                await connection.SeekAsync(newSpan);
            else
                await connection.SeekAsync(newSpan);

            return $"Playing from {newSpan:mm\\:ss}";
        }

        public Task<string> Clean()
        {
            var queueToList = queue.ToList();
            int removed = 0;

            foreach (var value in queueToList)
                if(connection.Channel.Users.FirstOrDefault(x => x.Id == value.id) == null)
                {
                    queueToList.Remove(value);
                    removed++;
                }

            var newQueue = new Queue<QueueData>();

            foreach (var value in queueToList)
                newQueue.Enqueue(value);

            queue = newQueue;
            return Task.FromResult($"Removed {removed} track(s)");
        }
    }
}
