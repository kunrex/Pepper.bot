using System;

namespace KunalsDiscordBot.Core.DiscordModels
{
    public struct CustomTuple<Item_1, Item_2>
    {
        public Item_1 Item1 { get ; private set; }
        public Item_2 Item2 { get ; private set; }

        public CustomTuple(Item_1 item1, Item_2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
