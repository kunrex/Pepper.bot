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
using DiscordBotDataBase.Dal.Models.Servers.Models.Music;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public sealed class VCPlayer 
    {
        private static readonly int Height = 30;
        private static readonly int Width = 40;

        public Queue<QueueData> Queue { get; private set; }
        public LavalinkGuildConnection Connection { get; private set; }
        public LavalinkExtension Lava { get; private set; }
        public LavalinkNodeConnection Node { get; private set; }

        public SimpleBotEvent OnDisconnect { get; private set; } = new SimpleBotEvent();

        private readonly MusicModuleData moduleData;

        public LavalinkTrack CurrentTrack { get => Connection.CurrentState.CurrentTrack; }
        private LavalinkTrack currentTrack;

        private string memberWhoRequested = string.Empty;

        private bool isPaused { get; set; }
        private bool isLooping { get; set; }
        private bool queueLoop { get; set; }
        private bool isConnected { get; set; } = false;

        private DiscordChannel BoundChannel { get; set; }
        private CancellationTokenSource InactivityCancellationToken { get; set; }

        public VCPlayer(MusicModuleData _moduleData, LavalinkNodeConnection nodeConnection, LavalinkExtension extension)
        {
            moduleData = _moduleData;
            Queue = new Queue<QueueData>();

            Node = nodeConnection;
            Lava = extension;
        }

        public async Task<string> Connect(DiscordChannel _channel, DiscordChannel _boundChannel)
        {
            if (isConnected)
                return "";

            isConnected = true;

            Connection = await Node.ConnectAsync(_channel);

            Connection.PlaybackFinished += OnSongFinish;
            Connection.TrackException += OnTrackError;
            Connection.TrackStuck += OnTrackStuck;
            Connection.PlaybackStarted += OnSongStart;

            BoundChannel = _boundChannel;

            return $"Joined <#{_channel.Id}> and bound to <#{_boundChannel.Id}> \nUse the `music play` command to play some music";
        }

        public async Task<string> Disconnect()
        {
            if (!isConnected)
                return "";

            isConnected = false;

            await Connection.DisconnectAsync();
            OnDisconnect.Invoke();

            if (InactivityCancellationToken != null)
            {
                if(!InactivityCancellationToken.IsCancellationRequested)
                    InactivityCancellationToken.Cancel();

                InactivityCancellationToken.Dispose();
            }
            return $"Left {Connection.Channel.Mention} succesfully";
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
                await BoundChannel.SendMessageAsync(new DiscordEmbedBuilder
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
                await BoundChannel.SendMessageAsync(new DiscordEmbedBuilder
                {
                    Description = "Current track got stuck",
                    Color = DiscordColor.Red
                });
            });

            return Task.CompletedTask;
        }

        private Task OnSongStart(LavalinkGuildConnection connect, TrackStartEventArgs args)
        {
            Task.Run(async () => await BoundChannel.SendMessageAsync(await NowPlaying()));

            return Task.CompletedTask;
        }

        public async Task<DiscordEmbedBuilder> StartPlaying(string search, string member, ulong id)
        {
            var loadResult = await Node.Rest.GetTracksAsync(search);

            if (loadResult == null || loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                return new DiscordEmbedBuilder().WithDescription($"Track search failed for {search}").WithColor(moduleData.color);

            if (Queue.Count == moduleData.maxQueueLength)
                return new DiscordEmbedBuilder().WithDescription($"Queue is at max length ({moduleData.maxQueueLength}), remove tracks or wait to add more").WithColor(moduleData.color);

            if (InactivityCancellationToken != null)//inactivity check was started
                InactivityCancellationToken.Cancel();

            if (currentTrack == null)
            {
                currentTrack = loadResult.Tracks.First();
                memberWhoRequested = member;

                await Connection.PlayAsync(currentTrack);

                return null;//handled by now playing function
            }
            else
            {
                var track = loadResult.Tracks.First();
                Queue.Enqueue(new QueueData { userName = member , id = id, track = track });
                return new DiscordEmbedBuilder().WithDescription($"Queued [{track.Title}]({track.Uri}) at index `{Queue.Count}`").WithColor(moduleData.color);
            }
        }

        public async Task<(bool, int)> QueuePlaylist(PlaylistTrack[] tracks, string member, ulong id)
        {
            for(int i = 0; i< tracks.Length;i++)
            {
                var loadResult = await Node.Rest.GetTracksAsync(tracks[i].URI);

                if (loadResult == null || loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    return (false, i);

                if (InactivityCancellationToken != null)//inactivity check was started
                    InactivityCancellationToken.Cancel();

                if (currentTrack == null)
                {
                    currentTrack = loadResult.Tracks.First();
                    memberWhoRequested = member;

                    await Connection.PlayAsync(currentTrack);
                }
                else
                {
                    var track = loadResult.Tracks.First();
                    Queue.Enqueue(new QueueData { userName = member, id = id, track = track });
                }
            }

            return (true, -1);
        }

        private async Task PlayNext(bool considerLoop = true)
        {
            if (isLooping && considerLoop)
            {
                await Connection.PlayAsync(currentTrack);
 
                return;
            }

            if (Queue.Count <= 0)
            {
                await BoundChannel.SendMessageAsync("Queue Finished");
                currentTrack = null;

                InactivityCancellationToken = new CancellationTokenSource();
                BeginInactivityCheck();
                return;
            }

            var search = Queue.Dequeue();
            memberWhoRequested = search.userName;

            await Connection.PlayAsync(search.track);

            currentTrack = search.track;
            if (queueLoop)//re add the search
                Queue.Enqueue(search);
        }

        private async void BeginInactivityCheck()
        {
            if (InactivityCancellationToken == null || InactivityCancellationToken.IsCancellationRequested)
                return;

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(moduleData.inactivityLength), InactivityCancellationToken.Token);
            }
            catch
            { }

            if (InactivityCancellationToken != null && InactivityCancellationToken.IsCancellationRequested)
            {
                if (isConnected)
                {
                    InactivityCancellationToken.Dispose();
                    InactivityCancellationToken = null;
                }

                return;
            }

            if (isConnected)
            {
                await BoundChannel.SendMessageAsync("Leaving due to inactivity").ConfigureAwait(false);
                await Disconnect();
            }
        }

        public async Task<string> Pause()
        {
            if (isPaused)
                return "Player already paused";

            await Connection.PauseAsync();
            isPaused = true;

            InactivityCancellationToken = new CancellationTokenSource();

            BeginInactivityCheck();
            return "Player Paused";
        }

        public async Task<string> Resume()
        {
            if (!isPaused)
                return "Player in't paused";

            await Connection.ResumeAsync();

            isPaused = false;
            InactivityCancellationToken.Cancel();
            return "Player Resumed";
        }

        public Task<string> Remove(int index)
        {
            if (Queue.Count > 0)
            {
                var queueToList = Queue.ToList();

                if ((index - 1) >= queueToList.Count || (index - 1) < 0)
                    return Task.FromResult("Index does not exist");

                queueToList.RemoveAt(index - 1);

                Queue.Clear();
                queueToList.ForEach(x => Queue.Enqueue(x));
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
            var title = $"Queue For __{Connection.Channel.Guild.Name}__";

            embeds.Add(new DiscordEmbedBuilder().WithTitle("Now Playing").WithColor(moduleData.color).WithDescription($"[{currentTrack.Title}]({currentTrack.Uri})"));
            DiscordEmbedBuilder currentEmbed = null;

            foreach(var value in Queue)
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
             .AddField("`Position`", $"{Connection.CurrentState.PlaybackPosition:mm\\:ss}")
             .AddField("`Next Track:` ", Queue.TryPeek(out QueueData result) ? $"[{Queue.Peek().track.Title}]({Queue.Peek().track.Uri})" : "Nothing", true)
             .AddField("`Paused`", isPaused.ToString(), true)
             .AddField("`Looping`", isLooping.ToString(), true)
             .AddField("`Queue Loop`", queueLoop.ToString(), true)
             .WithThumbnail(thumbnailURL, Height, Width)
             .WithColor(moduleData.color);

            return Task.FromResult(embed);
        }

        public Task<string> Move(int trackToMove, int newIndex)
        {
            if(Queue.Count <= 1)
                return Task.FromResult("Queue only has 1 elemant");
            if (trackToMove < 1 || trackToMove > Queue.Count || newIndex == trackToMove || newIndex < 1 || newIndex > Queue.Count)
                return Task.FromResult("Invalid position(s), use the `queue` command to view the queue and enter valid position(s)");

            var tracks = Queue.ToList();

            var track = tracks[trackToMove - 1];
            tracks.RemoveAt(trackToMove - 1);

            tracks.Insert(newIndex - 1, track);

            Queue.Clear();
            tracks.ForEach(x => Queue.Enqueue(x));

            return Task.FromResult("Moved Track");
        }

        public Task<string> ClearQueue()
        {
            if (Queue.Count == 0)
                return Task.FromResult("Queue is already empty");

            Queue.Clear();
            return Task.FromResult("Queue cleared");
        }

        public async Task Skip() => await Connection.StopAsync();

        public async Task<string> Seek(TimeSpan span, bool relative = false)
        {
            if (span > TimeSpan.FromSeconds(0) ? (relative ? span + Connection.CurrentState.PlaybackPosition > currentTrack.Length  : span > currentTrack.Length) : (relative ? span + Connection.CurrentState.PlaybackPosition < TimeSpan.FromSeconds(0) : span < TimeSpan.FromSeconds(0)))
                return "Cannot play from specified position";

            var newSpan = relative ? Connection.CurrentState.PlaybackPosition + span : span;
            if (relative)
                await Connection.SeekAsync(newSpan);
            else
                await Connection.SeekAsync(newSpan);

            return $"Playing from {newSpan:mm\\:ss}";
        }

        public Task<string> Clean()
        {
            var queueToList = Queue.ToList();
            int removed = 0;

            foreach (var value in queueToList)
                if(Connection.Channel.Users.FirstOrDefault(x => x.Id == value.id) == null)
                {
                    queueToList.Remove(value);
                    removed++;
                }

            Queue.Clear();
            queueToList.ForEach(x => Queue.Enqueue(x));
            return Task.FromResult($"Removed {removed} track(s)");
        }
    }
}
