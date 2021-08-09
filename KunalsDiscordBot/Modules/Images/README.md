# Image Commands

Pepper contains a dedicated module for image commands. This can be used to create memes using pre-built formats and much more.
Currently this uses `System.Drawing`, but of course you can change the API if needed.

## The ImageGraphic class
`ImageGraphic.cs` is a wrapper around the System.Drawing implementation. It consists of a `Image` and `Graphics`, both created during initiliasiation. The class also inherits from `GraphicDisposable.cs`, allowing the usage of `using statements` to clear the memory associated with the variable after use.
```cs
using (var graphicalImage = new ImageGraphic(...))
{

}
```
### Creating Image Graphics
1. From File Paths
```cs
using (var graphicalImage = new ImageGraphic(Path.Combine("X", "Y", "Z")))
{

}
```

2. From a stream
```cs
//create a image graphic from a url.

using (var client = new WebClient())
{
  using (var graphicalImage = new ImageGraphic(new MemoryStream(client.DownloadData("url here"))))
  {

  }
}
```

### Drawing 

```cs 
* DrawString(string message, int x, int y, int length, int breadth, Font font, Brush brush)
* DrawString(string message, int x, int y, Font font, Brush brush)
* DrawImage(ImageGraphic other, int x, int y, RectangleF rect, GraphicsUnit unit)
```

### Image Manipulation

```cs
*  Resize(int width, int height)
*  RotateFlip(RotateFlipType rotateFlipType)
*  Invert()
```

### Conversion
```cs
* ToMemoryStream()
```

## Creating a new Format-Based Image Command

### Creating the required EditData

To create a format based new image command we first need an image. You can find any for this purpose. After you have an image add it to the folder: `KunalsDiscordBot/Modules/Images/Images`.

Copy the file name (including the extension) of the image as its used in places.

Navigate back to the parent folder of the one containing the images and find the `ImageData.json` file. This file contains information about images such as the file name and pixel co-ordinates at where we draw text or images.

To add your new image here, add a new element to the **edits** array.
the format for the elemant is 
```json
{
  "fileName": "",
  "x": [  ],
  "y": [  ],
  "size": [  ],
  "length": [  ],
  "breadth": [  ],
},
```
**What these values are**
1. `fileName` : The file name used as the base for this command, this must be exactly the same (including the extension). This also acts as a kind of identifier.
2. `x` : The x values of each coordinates in order
3. `y` : The y values of each coordinates in order
4. `size` : For drawing text, this is the size of the text drawn. For drawing images it is the size of the image itself.
5. `length` : This is primarily for drawing texts and preventing a text overflow
6. `breadth` : The same as `length`.

The `length` and `breadth` define a rectangle in which the text is drawn.

For example:
```json
{
  "fileName": "yesno.png",
  "x": [ 358, 358 ],
  "y": [ 12, 295 ],
  "length": [ 324, 324 ],
  "breadth": [ 243, 243 ],
  "size": [ 30, 30 ]
}
```
In code this would translate to 2 rects.
1. A rect with a top left coordinate of `{358, 12}` with length `324` and breadth `243`, in which the text drawn will be of size `30`
2. A rect with a top left coordinate of `{358, 295}` with length `324` and breadth `243`, in which the text drawn will be of size `30`

Note: The coordinates and dimensions are in pixels.

### Creating a new command
Im gonna recreate the `yesno` image command, as thats the onw I used for the `EditData` demo as well.
We can start by creating a new `async Task` with the `CommandContext` and the `[Command]` attribute
```cs
[Command("yesno")]
public async Task YesNo(CommandContext ctx)
{
            
}
```
We're gonna be drawing some text on top of the image. For this we first need a string parameter. We also now need a reference to the image file itself. The `[WithFile]` attribute comes in handy here.
```cs
[Command("CommandName")]
[WithFile("yesno.png")]//allows us to get a reference to the file
public async Task YesNo(CommandContext ctx, [RemainingText] string message)
{
            
}
```
The `[RemainingText]` tells the command to include all text including spaces.

Now that thats done, its time to split the sentences. The `yesno` command requires 2 sentences splitted using `,`'s. We can use the `string.Split()` method here.
```cs
 string[] sentences = message.Split(',');
```

Now that we have the sentences, lets access the image file as well as the edit data associated with it. This can be done using these lines of code
```cs
string fileName = service.GetFileByCommand(ctx);
string filePath = Path.Combine("Modules", "Images", "Images", fileName);

EditData editData = service.GetEditData(fileName);
```
`service` is an `ImageService` instance injected through DI into the ImageModule. The `GetFileByCommand` uses the `[WithFile]` attribute mentioned above to get the image file associated with the command.
After we have the file name we can get the path to the file and the edit data as well.

At this point we can create the `ImageGraphics` needed.
```cs
using (var graphicalImage = new ImageGraphic(filePath))
{

}
```
and use a for loop to draw the sentences on the image
```cs
using (var graphicalImage = new ImageGraphic(filePath))
{
  for (int i = 0; i < sentences.Length; i++)//loop through all the sentences
  {
    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);//create a font and brush.

    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);//draw the string on the image.
  }
}
```
This is where the edit data came into play. It stored the value in pixels used by the command to draw the strings.

Finally, can send the image as a file to discord
```cs
using (var graphicalImage = new ImageGraphic(filePath))
{
  for (int i = 0; i < sentences.Length; i++)//draw sentences
  {
    service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

    await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
  }

 using (var ms = await graphicalImage.ToMemoryStream())//convert the image into a memory stream
    await new DiscordMessageBuilder()
      .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
      .SendAsync(ctx.Channel);//send it on discord
}
```

The final product is
```json
{
  "fileName": "yesno.png",
  "x": [ 358, 358 ],
  "y": [ 12, 295 ],
  "length": [ 324, 324 ],
  "breadth": [ 243, 243 ],
  "size": [ 30, 30 ]
}
```

```cs
[Command("Yesno")]
[WithFile("yesno.png")]
public async Task YesNo(CommandContext ctx, [RemainingText] string message)
{
  string[] sentences = message.Split(',');

  string fileName = service.GetFileByCommand(ctx);
  string filePath = Path.Combine("Modules", "Images", "Images", fileName);

  EditData editData = service.GetEditData(fileName);

  using (var graphicalImage = new ImageGraphic(filePath))
  {
     for (int i = 0; i < sentences.Length; i++)
     {
         service.GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

        await graphicalImage.DrawString(sentences[i], editData.x[i], editData.y[i], editData.length[i], editData.breadth[i], drawFont, drawBrush);
    }

   using (var ms = await graphicalImage.ToMemoryStream())
       await new DiscordMessageBuilder()
           .WithFiles(new Dictionary<string, Stream>() { { fileName, ms } })
           .SendAsync(ctx.Channel);
  }
}
```
You can add a few checks, like to see if the appropriate amount of sentences were entered or to see if the sentence doesn't cross a character limit etc etc.

<image src="GitHubImages/amogus.png" width = 300>
