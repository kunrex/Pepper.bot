# Music Commands
Pepper uses LavaLink which is originally written in JDS to play music.
you can find more info about LavaLink here => https://github.com/freyacodes/lavalink

The **VCPlayer** class handles all music in a Server. it itself does not inherit anything.
**MusicCommands** holds a reference to all players currently in the bot, the players are identified by the guild id they belong to. Thus each guild cannot have more than
one player.

Each player has a **Queue\<string>** to represent the queue in the server, this can also be a **Queue\<LavalinkTrack>**, but for simplictly I did not do so.

The player has the following commands to manipulate the queue
* PlayNext
* Remove
* Move
* Clear
* Skip
* QueueLoop

The player has the following commands to manipulate the current track
* StartPlaying
* Pause
* Resume
* Seek or PlayFrom
* Loop

Combined the player provides a simple, easy to use and robust system to vibe with some tunes.
