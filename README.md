# Pepper.bot

Pepper is a discord bot I made a while back to learn. Its written in C#.
It's not a public bot and isn't meant to handle large number of servers as it does not have shards implemented yet although the services are dynamic enough to do so.

<img src="Images/Logo.jpg" width=200>

## Table Of Contents
* General Info
* Technologies
* How to Set It Up?
* Plans for the future

## General Info
__Modules :__

Module | Description
--- | --- |
General | Commands for general purposes, this also includes config commands for servers |
Moderation | Commands to moderate a server, including things like role and member management |
Soft Moderation | Commands for soft moderation like infractions, changing nicknames etc.|
Games | Commands to play games with other server members, Games inlcuded => TicTacToe, RockPaperScissors, Connect4 and Battleship with UNO in developement|
Fun | Commands for general fun. Includes Reddit based commands|
Music | Commands to play music on the server, has functionality for a queue and mutlpie commands to control it|
Images | Commands to make your own memes and other image mdeia with pre built meme formats.|
Math | Commands for math problems like solving and graphing linear equations|
Currency | Commands for a functioning currency system. Each user has their own profile which itself contains coins, levels, items and more|

Does not have seperate prefix' for each server, but will look into that. The bot is in developement, things I'm working on right now is
1. The Currency System.
2. UNO

## Technologies

* .NETCore 3.1
* .NETStandard 2.1
* <a href="https://github.com/DSharpPlus/DSharpPlus" target="_blank">DSharpPlus 4.0.1</a>
* Microsoft.EntityFrameWorkCore 5.0.7
* <a href="https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.Lavalink" target="_blank">DSharpPlus.LavaLink</a> 
* SQlite
* <a href="https://github.com/sirkris/Reddit.NET" target="_blank">Reddit.NET</a>
* <a href="https://denzven.pythonanywhere.com/" target="_blacnk">Denzven Graphing API</a>

## How to Set It Up?

### Cloning The Repository
1. Clone the repo on your local system using the following command line
```
$ git clone https://github.com/kunrex/Pepper.bot
```

### Creating and Setting up The Bot

1. Open a new Browser window and search <a href="https://discord.com/developers/docs/intro" target="_blank">Discord Developer Portal</a>. 
2. Navigate to the Applications window and select `New Application`. This application is your bot
3. Give the application a name and hit `Create`. You can now fill in the other details of the bot like the description and give the bot a pfp.
4. Navigate to **Bot** tab, click `Click To Reveal Token` and copy whatever appears. This is the token for your discord bot and is used in code. **DO NOT** show anyone this token.

#### Creating the Config File
5. Come back to the project on your system and find the folder called `KunalsDiscordBot` and create a new json file called `Config.json` exactly. You can change this name later if needed. This is gonna be the config file for the bot. Open the file and paste the following lines and fill in the appropriate values.
```json
{
  "token": "paste your client token here from the developer portal here",
  "prefixes": [ "Assign the prefix' of your bot here" ],
  "dms": true,
  "timeOut": 60,
  "KunalsID": "744901615919562813"
}
```

**What these values are**
1. `token`: The client token for your bot.
2. `prefixes`: The default prefixes for your bot.
3. `dms`: Are commands sent through direct messages are processed or not
4. `timeOut`: The default timeout (in seconds) for the interactivity for your bot. The value be overriden wherever nececarry but defaults to the value here.
5. `KunalsID`: **Only For Dev** The id the bot pings if something goes wrong. Will be removed.

This is all you need to do to set up the Discord Bot itself.

### Setting up Music Commands with LavaLink
Pepper uses <a href="https://github.com/DSharpPlus/DSharpPlus/tree/master/DSharpPlus.Lavalink" target="_blank">DSharpPlus.LavaLink</a>  for music commands. 
Find the LavaLink.jar file at `KunalsDiscordBot/Modules/Music`. open a cmd prompt or terminal instance at this path and run
```
java -jar Lavalink.jar
```
This runs the jar file.
The bot comes with a set up `application.yaml` file to configure LavaLink so it should run locally on your device. You can look into <a href="https://github.com/DSharpPlus/DSharpPlus" target="_blank">LavaLinks repository</a> or the <a href="https://dsharpplus.github.io/articles/audio/lavalink/setup.html" target="_blank">DSharpPlus tutorial</a> for more info.

### Reddit Services
Pepper uses <a href="https://github.com/sirkris/Reddit.NET" target="_blank">Reddit.NET</a> for all things reddit. This is primarily used in the FunModule but can be built upon for more complex Reddit related commands.
#### __Settings Up the Reddit App__
1. Open up the <a href="https://www.reddit.com/prefs/apps" target="_blank">Apps</a> page on Reddit and hit `create an app` at the bottom.
2. Give the app a name and a description. Under `App Type` choose `script`. The `Redirect URL` can really be any valid URL. After that you can just hit `Create App`.
3. Its gonna redirect your page that looks something like this.
<img src="Images/RedditExample.png" width = 400> 

We need 2 things from here, your `client app id` (The set of characters under `personal use script`) and `client secret`.

4. Apart from those 2 we also need a `refreshToken`. Now I have to admit getting this is a bit trickier than the appId and secret. You can either follow the instructions at <a href="https://github.com/reddit-archive/reddit/wiki/OAuth2#authorization" target="_blank">here</a> or just use this simple <a href="https://not-an-aardvark.github.io/reddit-oauth-helper/" target="_blank">link</a>. 

#### Create the RedditConfig File
5. After you have all 3 of this we can create the RedditConfig file. Find the folder `KunalsDiscordBot/Reddit/` and create a new json file called `RedditConfig.json` exactly. You can change the name later if needed. This is the config for your Reddit app which is used by Pepper. 
Copy Paste the following lines into the json file and fill in the appropriate details wherever nececarry.
```json
{
  "appId": "paste your app id here",
  "appSecret": "paste your app secret here",
  "refreshToken": "paste your refresh token here",
  "postLimit" :  50
}
```

**What these values are**
1. `appId`: The appId of your reddit app.
2. `appSecret`: The appSecret of your reddit app.
3. `refreshToken`: The refresh token of your reddit app
4. `postLimit`: The maximum amount of posts the bot collects from each of the filter types (Top, Hot and New) from registered subreddits (r/memes, r/animals and r/aww). These posts are stored when the bot starts and accessed when a command is executed.

You can look into the Readme.md at **KunalsDiscordBot/Reddit/** for more info on the Reddit implementation.

### Setting up the Database for Currency Commands
The github repo does not include the Migrations folder nor does it include the Database itself. So you need to create them when you clone the repo locally if you want currency commands to work.

#### Prerequisites
Copy the path of the folder called `KunalsDiscordBot` and open a cmd prompt or terminal instance at this path.
First make sure you have dotnet-ef installed by running.
```
dotnet tool install -global dotnet-ef
```
Also make sure you have the package `Microsoft.EntityFrameWorkCore.Design` installed through the nuget manager or the following cmd line
```
dotnet add package Microsoft.EntityFrameWorkCore.Design
```

#### Creating the Database and Migrations
We can now create a migration using
```
dotnet-ef migrations add InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
This line, creates a new migration called `InitialCreate` in the project called `DiscordBotDataBase.Dal.Migrations`.

the last part of the line line uses the DbContext `DataContext` in the namespace `DiscordBotDataBase.Dal`. 
If you have done everything correctly and the build succeedes its time to actually create our database using the following command
```
dotnet-ef database update InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
Its the same as before but instead of `migrations add`, we use `database update`. The other parameters are the same.
If you get no errors and the build succeedes then you should see a file called `Data.db` appear in the folder called `KunalsDiscordBot` considering you haven't changed anything.
If you do then great work! The databse has been set up succesfully.

Any time you make any chages to the Database Models you will be needed to run these last 2 commands in that specific order to add the new migrations and update the database
Although you would change the names of the Migrations on each update. for this example we used `InitialCreate`. You would put in a different value every time you create something new.
You can look into the <a href="https://docs.microsoft.com/en-us/ef/core/get-started/overview/first-app?tabs=netcore-cli" target="_blank">Microsoft Docs</a> for SQLite for more info.

## Plans for the future

I'm not quite done with the bot. Right now my goal is to finish the currency system linked to the database and the UNO game
I can add more games, images and modules. After all this is just to learn. Am also looking to add special prefix' for each server which includes a custom command handler. 

For More info you can contact me on Discord

![](Images/DiscordIcon.png)
