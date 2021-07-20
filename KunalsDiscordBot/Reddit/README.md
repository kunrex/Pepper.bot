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
For the purposes of this tutorial, I'll be subscribing to r/showerthoughts.
First we can create a List for all the posts
```cs
List<Post> showerThoughts = new List<Post>();
```

We can then use the `SubRedditSetUp` method. It takes in a `string` for the subreddit name and the `EventHandler<PostsUpdateEventArgs> action` for when a 
new post is added and returns a `List<Post>`
```cs
showerThoughts = SubRedditSetUp("showerthoughts", (s, e) => {
  if(showerThoughts == null)
    return;

  foreach (var post in e.Added)//all the new posts
    if (post.Listing.URL.EndsWith(".png") || post.Listing.URL.EndsWith(".jpeg") || post.Listing.URL.EndsWith(".gif"))//if its a valid discord posts
    {
      memes.RemoveAt(0);//cycle the posts
      memes.Add(post);
    }
});
```
You can wrap the event handler in a method to make things cleaner
```cs
showerThoughts = SubRedditSetUp("showerthoughts", (s, e) => OnShowerThoughsAdded(s, e));

private void OnShowerThoughsAdded(object sender, PostsUpdateEventArgs e)
{
  if(showerThoughts == null)
    return;

  foreach (var post in e.Added)//all the new posts
    if (post.IsValidDiscordPost())//if its a valid discord posts, ie: is an image or a gif
    {
      memes.RemoveAt(0);//cycle the posts
      memes.Add(post);
    }
}
```
Now when the bot starts. It will collect 50 (since thats the `postLimit` in the config file by default) posts from  r/showerthoughts. You can now create
a method to get a random posts
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
