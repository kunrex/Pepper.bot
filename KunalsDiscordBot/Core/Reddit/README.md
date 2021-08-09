# Reddit Services
As Mentioned in the primary Readme, Pepper uses <a href="https://github.com/sirkris/Reddit.NET" target="_blank">Reddit.NET</a> for all things reddit.

### The Reddit Client
```cs
 public RedditClient client { get; private set; }
```
The Reddit client is the client we use to access anything from Reddit. Its directly linked to our Reddit app.
#### Basic Reddit related things 
1. Accessing a subreddit.
```cs
var subReddit = client.Subreddit("subRedditName").About();
```

2. Accessing posts from a subreddit
To acesss all the posts from a subreddit we use
```cs
var subReddit = client.Subreddit("subRedditName").About();

var posts = subReddit.Posts;
```
Keep in mind, this is not a collection, just a singular instance. Reddit filters posts based on 3 criterias. `Hot`, `New` and `Top`. To Access the posts from 
one of these criteria.
```cs
var subReddit = client.Subreddit("subRedditName").About();

var posts = subReddit.Posts;

//gather the respective posts
var hot = posts.Hot;
var new = posts.New;
var top = posts.Top;
```
`hot`, `new` and `top` are collections of posts, so we can use Linq to filter out posts based on a criteria
```cs
var subReddit = client.Subreddit("subRedditName").About();

var posts = subReddit.Posts;

//gather the respective posts
var hot = posts.Hot;

//gather all posts that aren't NSFW
var filtered = hot.Where(x => !x.NSFW).ToList();
```

### "Subscribing" To subreddits
The previous method access' a subreddit during runtime and gathers a post. While it works its also slow. At times it can take upto 10 seconds 
or even more which is understandable.

However for some commands, we don't want that. Pepper "subscribes" to specific subreddits.

For Example, Pepper has a built it `meme` command in the `FunModule`. For this command specifically, the RedditApp subscribes to `r/memes`. Which just means
that when the application starts, the app collects 50 memes from each of the sorting criteria (Hot, New and Top) from `r/memes`. It also subscribes to the
events which are called when a new meme is uploaded on the subreddit. 

When the meme command is executed, all it does is get a random meme from the already stored ones and sends it on discord.

#### Subscribing To More Subreddits
For the purposes of this tutorial, I'll be subscribing to `r/showerthoughts`.
First we can create a `RedditPostCollection` which is a custom wrapper
```cs
private RedditPostCollection showerThoughts { get; set; }
```

Next in the `SetUpCollections` collections method, we initialise the post collection, passing in the name of the subreddit itself.
We can now call the `Start` method of the post collection. This deals with adding all the posts as well as subscriving to events.
```cs
showerThoughts = new RedditPostCollection("showerthoughts");//create a new post collection
await showerThoughts.Start(client, configuration);//collect the posts
```
Now when the bot starts. It will collect a total of 150 (since the `postLimit` is 50 ad theres 3 sorting criteria (50 x 3)) posts from  r/showerthoughts. 
You can now create a method to get a random post
```cs
public Post GetShowerThought() => showerThoughts[new Random().Next(0, showerThoughts.Count)];
```
and call this from a command
```cs
[Group("Fun")]
public class FunModule : BaseCommandModule
{
  //more commands
  
  [Command]
  public async Task ShowerThought(CommandContext ctx) 
  {
    var post = redditApp.GetShowerThought();

    await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
    {
      Title = post.Title,
      ImageUrl = post.Listing.URL,
      Url = "https://www.reddit.com" + post.Permalink,
    });
  }
}
```
