# Pepper.bot

Pepper is a discord bot I made a while back to learn. Its written in C# and has the functinality from moderating discord servers to playing music and games online.
It's not a public bot and isn't meant to handle large number of servers although most of the services are dynamic enough to do so.

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
General | Set Of Commands for general purposes, this also includes config commands for servers |
Moderation | Set Of Commands to moderate a server, including role and member management |
Soft Moderation | Set Of Commands for soft moderation like changing nick names and muting and deafening members in voice channels|
Games | Set Of Commands to play games with other server members, Games inlcuded => TicTacToe, RockPaperScissors, Connect4 and Battleship|
Fun | Set Of Commands for general fun|
Music | Set Of Commands to play music on the server, has functionality for a queue and mutlpie commands to control it|
Images | Set Of Commands to make your own memes and other image mdeia with pre built meme formats|
Math | Set Of Commands for math problems like solving linear equations|
Currency | Set Of Commands for a functioning currency system. Each user has their own profile which itself contains coins, levels, items and more|

Does not have seperate prefix' for each server, but will look into that. The bot is in developement, things I'm working on right now is
1. The Currency System.
2. UNO

## Technologies

* .NETCore 3.1
* .NETStandard 2.1
* DSharpPlus 4.0.1
* Microsoft.EntityFrameWorkCore 5.0.7
* DSharpPlus.LavaLink (https://github.com/freyacodes/lavalink)
* SQlite
* Reddit.NET
* Denzven Graphing API (https://denzven.pythonanywhere.com/)

## How to Set It Up?

1. Clone the repo on your local system and open it up on your prefered IDE or Code Editor.
### Creating and Setting up The Bot
2. Open a new Browser window and search **Discord Developer Portal** or click the link below\n
https://discord.com/developers/docs/intro
3. Navigate to the Applicates window and select **New Application**. This application is your bot
4. Fill in the details for the bot like the name and description.
5. Navigate to Bot tab and click **Click To Reveal Token**.This copies your token and is needed to run your bot. **DO NOT** show anyone your token.

#### Creating the Config File
6. Come back to the project and find the folder **KunalsDiscordBot** and create a new json file called **Config.json** exactly. You can change the name later if needed. This is gonna be the config file for the bot. Open the file and paste the following lines and fill in the appropriate values.
```
{
  "token": "paste your client token here",
  "prefixes": [ "Assign the prefix' of your bot here" ],
  "dms": true,
  "timeOut": 60,
  "KunalsID": "744901615919562813"
}
```

**What these values are**
1. **token**: The client token for your bot.
2. **prefixes**: The default prefixes for your bot.
3. **dms**: Are commands sent through direct messages are processed or not
4. **timeOut**: The default timeout (in seconds) for the interactivity for your bot. The value be overriden wherever nececarry but defaults to the value here.
5. **KunalsID**: **Only For Dev** The id the bot pings if something goes wrong. Will be removed

### 7. Setting up Music Commands with LavaLink
For Music, find the LavaLink.jar file at **KunalsDiscordBot/Modules/Music**. open a cmd prompt or terminal instance at this path and run
```
java -jar Lavalink.jar
```
The bot comes with a set up **application.yaml** file to configure LavaLink so it should run locally on your device. You can look into LavaLinks official Repo (https://github.com/freyacodes/lavalink) or the DSharpPlus tutorial for more info to set it up (https://dsharpplus.github.io/articles/audio/lavalink/setup.html)

### 8. Reddit Services
Pepper uses Reddit.NET (https://github.com/sirkris/Reddit.NET) for all things reddit. This is primarily used in the FunModule for the `post`, `meme`, `animals` and `awww` command but can be built upon for more complex Reddit related commands.
#### __Settings Up the Reddit App__
1. Go to your apps page on reddit at https://www.reddit.com/prefs/apps and hit the **create an app** button at the bottom.
2. Give the app a name and a description. Under **App Type** choose **script**. The **Redirect URL** can really be any valid URL. After that you can just hit **Create App**
3. Its gonna redirect your page that looks something like this.
<img src="Images/RedditExample.png" width = 400> 

We need 2 things from here, your **client app id** (The set of characters under **personal use script**) and **client secret**.

4. Apart from those 2 we also need a **refreshToken**. Now I have to admit getting this is a bit trickier than the appId and secret. You can either follow the instructions at https://github.com/reddit-archive/reddit/wiki/OAuth2#authorization or just use this simple link: https://not-an-aardvark.github.io/reddit-oauth-helper/. 

#### Create the RedditConfig File
5. After you have all 3 of this we can create the RedditConfig file. Find the folder **KunalsDiscordBot/Reddit/** and create a new json file called **RedditConfig.json** exactly. You can change the name later if needed. This is the config for your Reddit app which is used by Pepper. 
Copy Paste the following lines into the json file and fill in the appropriate details wherever nececarry.
```
{
  "appId": "paste your app id here",
  "appSecret": "paste your app secret here",
  "refreshToken": "paste your refresh token here",
  "postLimit" :  50
}
```
What is **postLimit**? postLimit is simply the maximum amount of posts the bot collects from each of the filter types (Top, Hot and New) from registered subreddits (r/memes, r/animals and r/aww).
We use this value to store memes and animals posts when the bot starts, instead of doing it on the spot when the command executes simply because its wayyy faster and efficient (like waaaayy faster. On the spot could take like 7 seconds while with the current set up it takes less than a second)

You can look into the Readme at **KunalsDiscordBot/Reddit/** for more info on the Reddit implementation but all the setting up has been done!

### 9. Setting up the Database for Currency Commands
The github repo does not include the **Migrations folder** nor does it include the Database itself. So you need to do this when you clone the repo locally if you want currency commands to work.

#### Prerequisites
Copy the path of the folder called **KunalsDiscordBot** and open a cmd prompt or terminal instance at this path.\n
First make sure you have dotnet-ef installed by running.
```
dotnet tool install -global dotnet-ef
```
Also make sure you have the package **Microsoft.EntityFrameWorkCore.Design** installed through the nuget manager or the following cmd line
```
dotnet add package Microsoft.EntityFrameWorkCore.Design
```

#### __Creating the Database and its migrations__
We can now create a migration using
```
dotnet-ef migrations add InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
This line, creates a new migration called "InitialCreate" in the project called "DiscordBotDataBase.Dal.Migrations" whose path is **../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj**
This is the same project we deleted the "Migrations" folder from earlier. If you change the project name then you must change it here as well.

the last part of the line line uses the DbContext called "DataContext" in the namespace "DiscordBotDataBase.Dal". Similarly to the migrations project, if you change the class or namespace names then you must do so here as well.
If you have done everything correctly and the build succeedes its time to actually create our database using the following command
```
dotnet-ef database update InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
Its the same as before but instead of **migrations add**, we use **database update**. The other parameters are the same.
If you get no errors and the build succeedes then you should see a file called Data.db appear in the folder called "KunalsDiscordBot" considering you haven't changed anything.
If you do then great work! The databse has been set up succesfully.

Any time you make any chages to the Database Models you will be needed to run these last 2 commands to update the databse and add the new migrations.
Although you would change the names of the Migrations on each update. for this example we used "InitialCreate". You would put in a different value every time you create something new.
I would reccomend reading up on these topics.

## Plans for the future

I'm not quite done with the bot and will finish it before making this repo public. Right now my goal is to finish the currency system linked to the database.
I can add more games, images and modules. After all this is just to learn. Am also looking to add special prefix' for each server but that may include a custom command handler 

For More info you can contact me on Discord

![](Images/DiscordIcon.png)
