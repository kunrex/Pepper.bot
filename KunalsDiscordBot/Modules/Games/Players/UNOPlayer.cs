using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using KunalsDiscordBot.Modules.Games.Complex.UNO;
using KunalsDiscordBot.Services.Images;

namespace KunalsDiscordBot.Modules.Games.Players
{
    public class UNOPlayer : DiscordPlayer
    {
        public static int maxImagesPerRow = 6;
        public static int widthInPixel = 168;
        public static int heightInPixel = 259;
        public static int gapInPxiels = 10;

        public DiscordChannel dmChannel { get; private set; }

        private List<Card> cards { get; set; }
        private IImageService imageService;

        public UNOPlayer(DiscordMember _member, IImageService service) : base(_member)
        {
            member = _member;
            imageService = service;
        }

        public void InitiliasePlayerCards(List<Card> _cards) => cards = _cards;

        public async override Task<bool> Ready(DiscordChannel channel)
        {
            dmChannel = channel;

            await dmChannel.SendMessageAsync("Cards recieved").ConfigureAwait(false);
            return true;
        }

        public async Task<DiscordEmbedBuilder> GetPlayerEmbed()
        {
            return new DiscordEmbedBuilder
            {
                Title = $"{member.Username}'s Cards"
            };
        }

        private byte[] GetPlayerCards()
        {
            int height = 0, width = 0, numInRow = 0;
            foreach(var card in cards)
            {
                width += widthInPixel + gapInPxiels;

                numInRow++;
                if (numInRow == maxImagesPerRow)
                    height += gapInPxiels;
            }

            var bitmap = imageService.GetNewBitmap(height, width);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                height = 0; width = 0; numInRow = 0;
                foreach (var card in cards)
                {
                    var pathList = new List<string>();
                    pathList.AddRange(Card.Path);
                    pathList.Add(card.fileName);

                    using (var imgGraphic = imageService.GetImageFromFile(Path.Combine(pathList.ToArray())))
                    {
                        graphics.DrawImage(imgGraphic, new Point(height, width));
                    }

                    numInRow++;
                    if (numInRow == maxImagesPerRow)
                        height += gapInPxiels;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, bitmap.RawFormat);
                    return ms.ToArray();
                }
            }
        }
    }
}
