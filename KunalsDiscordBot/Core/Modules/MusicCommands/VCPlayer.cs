using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

using KunalsDiscordBot.Core.Events;

namespace KunalsDiscordBot.Core.Modules.MusicCommands
{
    public sealed class VCPlayer 
    {
        private static readonly int Height = 30;
        private static readonly int Width = 40;

        public ulong guildID { get; private set; }
        public Queue<QueueData> queue { get; private set; }
        public LavalinkGuildConnection connection { get; private set; }
        public LavalinkExtension lava { get; private set; }
        public LavalinkNodeConnection node { get; private set; }

        public SimpleBotEvent OnDisconnect { get; private set; } = new SimpleBotEvent();

        private string memberWhoRequested = string.Empty;
        private LavalinkTrack currentTrack { get => connection.CurrentState.CurrentTrack; }

        private bool isPaused { get; set; }
        private bool isLooping { get; set; }
        private bool queueLoop { get; set; }
        private bool isConnected = false;

        private DiscordChannel boundChannel { get; set; }

        public VCPlayer(ulong id, LavalinkNodeConnection nodeConnection, LavalinkExtension extension)
        {
            guildID = id;
            queue = new Queue<QueueData>();

            node = nodeConnection;
            lava = extension;
        }

        public async Task<string> Disconnect()
        {
            if (!isConnected)
                return "";

            isConnected = false;

            await connection.DisconnectAsync();
            OnDisconnect.Invoke();

            return $"Left {connection.Channel.Mention} succesfully";
        }

        public async Task<string> Connect(DiscordChannel _channel, DiscordChannel _boundChannel)
        {
            if (isConnected)
                return "";

            isConnected = true;

            connection = await node.ConnectAsync(_channel);

            connection.PlaybackFinished += OnSongFinish;
            boundChannel = _boundChannel;

            return $"Joined <#{_channel.Id}> and bound to <#{_boundChannel.Id}> \nUse the `play` command to play some music";
        }

        private async Task OnSongFinish(LavalinkGuildConnection connect, TrackFinishEventArgs args) => await PlayNext();

        public async Task<DiscordEmbedBuilder> StartPlaying(string search, string member, ulong id)
        {
            if (connection == null)
                return new DiscordEmbedBuilder().WithDescription("LavaLink is not connected.");

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                return new DiscordEmbedBuilder().WithDescription($"Track search failed for {search}");

            if (currentTrack == null)
            {
                var track = loadResult.Tracks.First();
                memberWhoRequested = member;

                await connection.PlayAsync(track);

                return new DiscordEmbedBuilder().WithDescription($"Playing [{track.Title}]({track.Uri})");
            }
            else
            {
                var track = loadResult.Tracks.First();
                queue.Enqueue(new QueueData { userName = member , id = id, track = track });
                return new DiscordEmbedBuilder().WithDescription($"Queue  [{track.Title}]({track.Uri}) at index `{queue.Count}`");
            }
        }

        private async Task PlayNext(bool considerLoop = true)
        {
            if (isLooping && considerLoop)
            {
                await connection.PlayAsync(currentTrack);

                var _embed = await NowPlaying();
                await boundChannel.SendMessageAsync(embed: _embed);

                return;
            }

            if (queue.Count <= 0)
            {
                await boundChannel.SendMessageAsync("Queue Finished");
                return;
            }

            var search = queue.Dequeue();
            memberWhoRequested = queue.Dequeue().userName;

            await connection.PlayAsync(search.track);

            if (queueLoop)//re add the search
                queue.Enqueue(search);

            await boundChannel.SendMessageAsync(await NowPlaying());
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

        public async Task<string> Remove(int index)
        {
            await Task.CompletedTask;

            if (queue.Count > 0)
            {
                var queueToList = queue.ToList();

                if((index - 1) >= queueToList.Count || (index - 1) < 0)
                    return "Index does not exist";

                queueToList.RemoveAt(index - 1);

                var newQueue = new Queue<QueueData>();

                foreach (var value in queueToList)
                    newQueue.Enqueue(value);

                queue = newQueue;
                return $"Removed at {index}";
            }
            else
                return "Queue is Empty";
        }

        public async Task<bool> Loop()
        {
            isLooping ^= true;
            await Task.CompletedTask;

            return isLooping;
        }

        public async Task<bool> QueueLoop()
        {
            queueLoop ^= true;
            await Task.CompletedTask;

            return queueLoop;
        }

        public Task<List<DiscordEmbedBuilder>> GetQueue()
        {
            List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();
            int index = 0, newEmbedIndex = 15;
            var title = $"Queue For __{connection.Channel.Guild.Name}__";

            DiscordEmbedBuilder currentEmbed = new DiscordEmbedBuilder().WithTitle(title).AddField("Now Playing",
                $"[{currentTrack.Title}]({currentTrack.Uri})");

            embeds.Add(currentEmbed);

            foreach(var value in queue)
            {
                if (index == 0)
                {
                    currentEmbed = new DiscordEmbedBuilder().WithTitle(title);
                    embeds.Add(currentEmbed);
                }

                currentEmbed.AddField($"[{value.track.Title}]({value.track.Uri})", $"Length: {value.track.Length:mm\\:ss}, Requested By: `{value.userName}`");

                index++;
                if (index == newEmbedIndex)
                    index = 0;
            }

            return Task.FromResult(embeds);
        }

        public async Task<DiscordEmbedBuilder> NowPlaying()
        {
            if (currentTrack == null)
                return new DiscordEmbedBuilder
                {
                    Title = $"Now Playing: Nothing",
                    Description = "Use the `play` command to play some music."
                }; 

            string thumbnailURL = $"https://img.youtube.com/vi/{await GetImageURL(currentTrack.Uri.AbsoluteUri)}/default.jpg";

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
             .WithThumbnail(thumbnailURL, Height, Width);

            return embed;
        }

        public async Task<string> Move(int trackToMove, int newIndex)
        {
            List<QueueData> tracks = queue.ToList();

            if(queue.Count <= 1)
                return "Queue is either empty or has only 1 search";
            if (trackToMove < 1 || trackToMove > queue.Count || newIndex == trackToMove || newIndex < 1 || newIndex > queue.Count)
                return "Invalid positions";

            var track = tracks[trackToMove - 1];
            tracks.RemoveAt(trackToMove - 1);

            tracks.Insert(newIndex - 1, track);

            Queue<QueueData> newQueue = new Queue<QueueData>();
            foreach (var _track in tracks)
                newQueue.Enqueue(_track);

            queue = newQueue;

            await Task.CompletedTask;
            return "Moved Track";
        }

        private async Task<string> GetImageURL(string url)
        {
            string imageURL = string.Empty, watchString = string.Empty;
            bool watchStringFound = false;
            int numOfSlashes = 0;

            foreach (var character in url)
            {
                if (character.Equals('/'))
                    numOfSlashes++;

                if (numOfSlashes == 3)
                {
                    if (watchStringFound)
                        imageURL += character;
                    else
                    {
                        watchString += character;

                        if (watchString == "/watch?v=")
                            watchStringFound = true;
                    }
                }
            }

            await Task.CompletedTask;
            return imageURL;
        }

        public async Task<string> ClearQueue()
        {
            if (queue.Count == 0)
                return "Queue is already empty";

            queue = new Queue<QueueData>();
            await Task.CompletedTask;

            return "Queue cleared";
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
