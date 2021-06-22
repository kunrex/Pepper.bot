# Image Commands

Pepper contains a dedicated module for image commands. This can be used to create memes using pre-built formats and much more.
Currently this uses **System.Drawing**, but of course you can change the API if needed.

## Creating a new Image Command
To create a new image command we first add the image. For this example we're gonna be adding Text to some images.
Find the folder called **Images**. Now come back to the folder containing this readme and find the 
**ImageData.json** file. This file contains information about images such as the file name and pixel co-ordinates at where we draw text or images.

To add your new image here, add a new element to the **Edits** array.
the format for the elemant is 
```
{
  "fileName": "",
  "x": [  ],
  "y": [  ],
  "size": [  ]
},
```
the file name is the name of the file containg the base image format.
**x** is the array of x positions in order and **y** is the array of y positions in order. **size** is the size of the text at this co-ordinate.
for example
```
{
  "fileName": "violence.png",
  "x": [ 362, 455 ],
  "y": [ 16, 32 ],
  "size": [ 23, 23 ]
},
```
In code we would get this as 2 co-ordinates. 
```
{x = 362, y = 16, size = 23} and {x = 455, y = 32, size = 23}
```
you can add how many every you want to.
Keep in mind these co-ordinates are the top left of the Text that will be added.

Coming to the code, make a new **public async Task** call it whatever you want. Add a CommandContext parameter since thats nececarry for all commands and another parameter
for the setences we need to add to the image. We can use the \[RemainingText] attribute as without it it only includes the first word.
Next add the \[Command] and \[Description] attribute to the method. Fill these as you need.
This is what we have by now
```
[Command("CommandName")]
[Description("CommandDescription")]
public async Task NewImageCommand(CommandContext ctx, [RemainingText] string message)
{
            
}
```
next we can get the sentences using the **GetSentences** method. it takes in a string for the message entered by the discord user and an int for the number of messages
to find.
Create a string for the filename we use for this image. After that use **Path.Combine** to make a path to this image like so
```
string fileName = "fileName.format";
string filePath = Path.Combine("Modules", "Images", "Images", fileName);
```
This path is considering you haven't made any changes.
After the path we can now access the **EditData** for the file from the JSON file earlier and Get the actual Image we need to edit.
To do so we use the **GetImages** method. Note: it has 2 out params, one for the image and the other for saveImage.
Since we're using files we first have to save the edited image locally in order to use it. After we have sent the message we can then save the original image without
the editting.
```
EditData editData = GetEditData(fileName);
GetImages(filePath, out Image image, out Image saveImage);
```
Its now time to create our graphics. The Graphics class is where the magic happens.
```
 Graphics graphics = Graphics.FromImage(image);//create a graphics from our Image
```
We're now ready to do all the editing.
Create a for loop make it loop through all the values of **sentences**.
We can now Get the Font and Brush we use for the edit. Heres where the **size** values from the json file comes in.
use the **GetFontAndBrush** to out the font and drawbrush needed for the edit. By default we're gonna pass in "Arial" as the font and Black as the color
```
GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);
```
now that we have our font we can use edit our image using
```
graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
```
your for loop should look something like this
```
for (int i = 0; i < sentences.Length; i++)
{
  GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

  graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
}
```
Now that we've edited our image lets save it and send it as a message in Discord. To send files we use the **DiscordMessageBuilder**.
```
image.Save(filePath);

using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
{
  var msg = await new DiscordMessageBuilder()
                        .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                        .SendAsync(ctx.Channel);

  fs.Close();//close the stream
}
```
Now that we've sent the edited image, we can save the original using
```
saveImage.Save(filePath);
```

This should be your entire method.
for this purpose
```
[Command("CommandName")]
[Description("CommandDescription")]
public async Task NewImageCommand(CommandContext ctx, [RemainingText] string message)
{
  string fileName = "fileName.format";
  string filePath = Path.Combine("Modules", "Images", "Images", fileName);

  EditData editData = GetEditData(fileName);
  GetImages(filePath, out Image image, out Image saveImage);

  Graphics graphics = Graphics.FromImage(image);//create a graphics from our Image

  for (int i = 0; i < sentences.Length; i++)
  {
    GetFontAndBrush("Arial", editData.size[i], Color.Black, out Font drawFont, out SolidBrush drawBrush);

    graphics.DrawString(sentences[i], drawFont, drawBrush, new PointF(editData.x[i], editData.y[i]));
  }
  
  image.Save(filePath);

  using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
  {
    var msg = await new DiscordMessageBuilder()
                          .WithFiles(new Dictionary<string, Stream>() { { fileName, fs } })
                          .SendAsync(ctx.Channel);

    fs.Close();//close the stream
  }

  saveImage.Save(filePath);
}
```

Congratulations, we've created a new Image Command! Now try it out on Discord.
