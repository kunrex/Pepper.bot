using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;

using KunalsDiscordBot.Services;
using DSharpPlus;
using System.Threading;
using DSharpPlus.Lavalink.EventArgs;

namespace KunalsDiscordBot.Services.Music
{
    public sealed class VCPlayer 
    {
        private static readonly int Height = 50;
        private static readonly int Width = 75;

        public ulong guildID { get; private set; }
        public Queue<string> queue { get; private set; }
        private Queue<string> memberswhoRequested { get; set; }
        public LavalinkGuildConnection connection { get; private set; }
        public LavalinkExtension lava { get; private set; }
        public LavalinkNodeConnection node { get; private set; }

        private string memberWhoRequested = string.Empty;

        private bool isPaused { get; set; }
        private bool isLooping { get; set; }
        private bool queueLoop { get; set; }
        private LavalinkTrack currentTrack;
        private bool isConnected = false;

        private DiscordChannel boundChannel { get; set; }

        public VCPlayer(ulong id, LavalinkNodeConnection nodeConnection, LavalinkExtension extension)
        {
            guildID = id;
            queue = new Queue<string>();
            memberswhoRequested = new Queue<string>();

            node = nodeConnection;
            lava = extension;
        }

        public async Task<string> Disconnect(string channelName)
        {
            if (!isConnected)
                return "";

            isConnected = false;

            if (!lava.ConnectedNodes.Any())
                return "LavaLink has not been established";

            if (connection == null)
                return "LavaLink is not connected";

            await connection.DisconnectAsync();
            return $"Left {channelName} succesfully";
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

        public async Task<string> StartPlaying(string search, string member)
        {
            if (connection == null)
                return "LavaLink is not connected.";

            if (currentTrack == null)
            {
                var loadResult = await node.Rest.GetTracksAsync(search);

                if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    return $"Track search failed for {search}";

                currentTrack = loadResult.Tracks.First();
                memberWhoRequested = member;

                await connection.PlayAsync(currentTrack);

                return "Playing...";
            }
            else
            {
                queue.Enqueue(search);
                memberswhoRequested.Enqueue(member);
                return $"Added {search} to queue";
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
                currentTrack = null;
                return;
            }

            currentTrack = null;

            var search = queue.Dequeue();
            memberWhoRequested = memberswhoRequested.Dequeue();

            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await boundChannel.SendMessageAsync($"Track search failed for {search}");
                await PlayNext();
                return;
            }

            currentTrack = loadResult.Tracks.First();

            await connection.PlayAsync(currentTrack);

            if (queueLoop)//re add the search
            {
                queue.Enqueue(search);
                memberswhoRequested.Enqueue(memberWhoRequested);
            }

            var embed = await NowPlaying();
            await boundChannel.SendMessageAsync(embed: embed);
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

                var newQueue = new Queue<string>();

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

        public async Task<DiscordEmbedBuilder> GetQueue()
        {
            string description;

            description = $"Now Playing: `{currentTrack.Title}`\n\n __Up Next__\n";
            if (queue.Count > 0)
            {
                int index = 1;
                var members = memberswhoRequested.ToArray();

                foreach (var search in queue)
                {
                    description += $"{index}. `{search}` \n Requested By: `{members[index - 1]}`\n";
                    index++;
                }
            }
            else
                description += "Queue is Empty";

            var embed = new DiscordEmbedBuilder
            {
                Title = "Queue",
                Description = description,
                Color = DiscordColor.Aquamarine
            };

            await Task.CompletedTask;
            return embed;
        }

        public async Task<DiscordEmbedBuilder> NowPlaying()
        {
            if (currentTrack == null)
            {
                var emptyEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Now Playing: Nothing",
                    Description = "Use the `play` command to play some music."
                };

                return emptyEmbed;
            }

            string id = await GetImageURL(currentTrack.Uri.AbsoluteUri);
            string thumbnailURL = $"https://img.youtube.com/vi/" + id + "/default.jpg";

            var thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = thumbnailURL,
                Height = Height,
                Width = Width
            };

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Now Playing: {currentTrack.Title}",
                Description = $"`Length:` {currentTrack.Length}\n `Author:` {currentTrack.Author}\n `Url:` {currentTrack.Uri.AbsoluteUri}",
                Color = DiscordColor.Aquamarine,
                Url = currentTrack.Uri.AbsoluteUri,
                Thumbnail = thumbnail
            };

            embed.AddField("`Requested By:` ", memberWhoRequested);
            embed.AddField("`Position`", $"{connection.CurrentState.PlaybackPosition.Minutes} : {connection.CurrentState.PlaybackPosition.Minutes}");
            embed.AddField("`Next Search:` ", queue.TryPeek(out string result) ? queue.Peek() : "Nothing", true);
            embed.AddField("`Paused`", isPaused.ToString(), true);
            embed.AddField("`Looping`", isLooping.ToString(), true);
            embed.AddField("`Queue Loop`", queueLoop.ToString(), true);

            return embed;
        }

        public async Task<string> Move(int trackToMove, int newIndex)
        {
            List<string> tracks = queue.ToList();

            if(queue.Count <= 1)
                return "Queue is either empty or has only 1 search";
            if (trackToMove < 1 || trackToMove > queue.Count || newIndex == trackToMove || newIndex < 1 || newIndex > queue.Count)
                return "Invalid positions";

            var track = tracks[trackToMove - 1];
            tracks.RemoveAt(trackToMove - 1);

            tracks.Insert(newIndex - 1, track);

            Queue<string> newQueue = new Queue<string>();
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

            queue = new Queue<string>();
            await Task.CompletedTask;

            return "Queue cleared";
        }

        public async Task Skip() => await connection.StopAsync();

        public async Task<string> Seek(TimeSpan span, bool relative = false)
        {
            if (relative ? span + connection.CurrentState.PlaybackPosition > currentTrack.Length : span > currentTrack.Length)
                return "Cannot play from specified position";

            if(relative)
                await connection.SeekAsync(connection.CurrentState.PlaybackPosition + span);
            else
                await connection.SeekAsync(span);

            return $"Playing from {connection.CurrentState.PlaybackPosition.Minutes}:{connection.CurrentState.PlaybackPosition.Seconds}";
        }
    }
}
