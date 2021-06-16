# Pepper.bot
Pepper is a discord bot I made a while back to learn. Its written in C# and has the functinality from moderating discord servers to playing music and games online.
It's not a public bot and isn't meant to handle large number of servers although most of the services are dynamic enough to do so.

<img src="Images/Logo.jpg" width=200>

## Table Of Contents
* General Info
* Technologies
* Setup

## General Info
__Modules :__

Module | Description
--- | --- |
Moderation | Set Of Commands to moderate a server, including role and member management |
Soft Moderation | Set Of Commands for soft moderation like changing nick names and muting and deafening members in voice channels|
Games | Set Of Commands to play games with other server members, Games inlcuded => TicTacToe, RockPaperScissors, Connect4 and Battleship|
Fun | Set Of Commands for general fun|
Music | Set Of Commands to play music on the server, has functionality for a queue and mutlpie commands to control it|
Images | Set Of Commands to make your own memes and other image mdeia with pre built meme formats|
Math | Set Of Commands for math problems like solving linear equations|
School | Makes online school easier, will probably be removed (maybe)|

Does not have seperate prefix' for each server, but will look into that

## Technologies

* C# => 
* DSharpPlus => 4.0.1
* Microsoft.EntityFrameWorkCore => 5.0.7
* LavaLink (https://github.com/freyacodes/lavalink)

## How to Set It Up?

1. Clone the repo on your local system and open it up on your prefered IDE or Code Editor.
2. Open a new Browser window and search **Discord Developer Portal** or click the link below
https://discord.com/developers/docs/intro
3. Navigate to the Applicates window and select **New Application**. This application is your bot
4. Fill in the details for the bot like the name and description.
5. Navigate to Bot tab and click **Click To Reveal Token**.This copies your token and is needed to run your bot. **DO NOT** show anyone your token.
6. Come back to the project and find the Config.json file (it is under the KunalsDiscordBot folder), there for the value called *token*, paste the token you copied from the dev portal. You can also change the prefix' to your liking.
7. For Music, find the LavaLink.jar file in *KunalsDiscordBot/Modules/Music*, open a cmd prompt or terminal instance at this path and run
```
java -jar Lavalink.jar
```
The bot comes with a set up *application.yaml* file to configure LavaLink so it should run locally on your device. You can look into LavaLinks official Repo (https://github.com/freyacodes/lavalink) or the DSharpPlus tutorial for more info to set it up (https://dsharpplus.github.io/articles/audio/lavalink/setup.html)

## Plans for the future

I'm not quite done with the bot and will finish it before making this repo public. My main goal is to add a currency system linked to a database and its in developement :D.
I can add more games, images and modules. After all this is just to learn

For More info you can contact me on Discord

![](Images/DiscordIcon.png)
