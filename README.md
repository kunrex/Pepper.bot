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

* .NETCore3.1
* .NETStandard2.1
* DSharpPlus => 4.0.1
* Microsoft.EntityFrameWorkCore => 5.0.7
* DSharpPlus.LavaLink (https://github.com/freyacodes/lavalink)

## How to Set It Up?

1. Clone the repo on your local system and open it up on your prefered IDE or Code Editor.
### Creating and Setting up The Bot
2. Open a new Browser window and search **Discord Developer Portal** or click the link below\n
https://discord.com/developers/docs/intro
3. Navigate to the Applicates window and select **New Application**. This application is your bot
4. Fill in the details for the bot like the name and description.
5. Navigate to Bot tab and click **Click To Reveal Token**.This copies your token and is needed to run your bot. **DO NOT** show anyone your token.
6. Come back to the project and find the **Config.json** file inside the folder called KunalsDiscordBot, there for the value called *token*, paste the token you copied from the dev portal. You can also change the prefix' to your liking. At this point you're done with the basic setting up of the bot.

### Setting up Music Commands with LavaLink
7. For Music, find the LavaLink.jar file at **KunalsDiscordBot/Modules/Music**. open a cmd prompt or terminal instance at this path and run
```
java -jar Lavalink.jar
```
The bot comes with a set up **application.yaml** file to configure LavaLink so it should run locally on your device. You can look into LavaLinks official Repo (https://github.com/freyacodes/lavalink) or the DSharpPlus tutorial for more info to set it up (https://dsharpplus.github.io/articles/audio/lavalink/setup.html)

### Setting up the Database for Currecny Commands
8. For the purposes of this set up we'll be remaking the entire database. You don't have to do this, but its advised to know how this is done if you add your own models and change things in the database and if you are might want to create a back up that you can revert to.

### Continue with SQLite
If you're on a Mac or Linux, we would use SQLite instead of an SQL Server.
Delete the **Migrations** folder under **DiscordBotDataBase.Dal.Migrations** and the file called **Data.db** under **KunalsDiscordBot**, we're gonna be remaking those.

Next copy the path of the folder called **KunalsDiscordBot** and open a cmd prompt or terminal instance at this path.\n
First make sure you have dotnet-ef installed by running.
```
dotnet tool install -global dotnet-ef
```
Also make sure you have the package **Microsoft.EntityFrameWorkCore.Design** installed through the nuget manager or the following cmd line
```
dotnet add package Microsoft.EntityFrameWorkCore.Design
```
after that we can add the migrations folder by running
```
dotnet-ef migrations add InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
This line, creates a new migration called "InitialCreate" in the project called "DiscordBotDataBase.Dal.Migrations" whose path is **../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj**
This is the same project we deleted the "Migrations" folder from earlier. If you change the project name then you must change it here as well.

the last part of the line line uses the DbContext called "DataContext" in the namespace "DiscordBotDataBase.Dal". Similarly to the migrations project, if you change the class or namespace names then you must do so here as well.
If you have done everything correctly and the build succeedes its time to actually create our database using the following command
```
dotnet-ef databse update InitialCreate -p ../DiscordBotDataBase.Dal.Migrations/DiscordBotDataBase.Dal.Migrations.csproj --context DiscordBotDataBase.Dal.DataContext
```
Its the same as before but instead of **migrations add**, we use **databse update**. The other parameters are the same.
If you get no errors and the build succeedes then you should see a file called Data.db appear in the folder called "KunalsDiscordBot" considering you haven't changed anything.
If you do then great work! The databse has been set up succesfully.

Any time you make any chages to the Database Models you will be needed to run these last 2 commands to update the databse and add the new migrations.
Although you would change the names of the Migrations on each update. for this example we used "InitialCreate". You would put in a different value every time you create something new.
I would reccomend reading up on these topics.

#### Using a SQL Server instead of SQLite
If you use a windows machine and want to use SQL Server then theres a couple of things to change before running the above commands.
Find the **StartUp.cs** file in the folder **KunalsDiscordBot**
you sould see a ConfigureServices method like so
```
 public void ConfigureServices(IServiceCollection services)
{
  Console.WriteLine("InConfigureServices");
  services.AddDbContext<DataContext>(options =>
  {
    //options.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DataContext;Trusted_Connection=True;MultipleActiveResultSets=true", X => X.MigrationsAssembly("DiscordBotDataBase.Dal.Migrations"));
                
    options.UseSqlite("Data Source=Data.db", x => x.MigrationsAssembly("DiscordBotDataBase.Dal.Migrations"));

    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
  });

  services.AddScoped<IProfileService, ProfileService>();

  BuildService(services);
}
```
Uncomment the commented line saying "options.UseSqlServer(....." and comment the line saying "options.UseSqlite(.....".
And thats it, now run the same commands listed in the above section from line *64*.
## Plans for the future

I'm not quite done with the bot and will finish it before making this repo public. My main goal is to add a currency system linked to a database and its in developement :D.
I can add more games, images and modules. After all this is just to learn. Am also looking to add special prefix' for each server

For More info you can contact me on Discord

![](Images/DiscordIcon.png)
