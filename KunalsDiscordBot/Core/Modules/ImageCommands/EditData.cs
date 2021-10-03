namespace KunalsDiscordBot.Core.Modules.ImageCommands
{
    public class EditData
    {
        public string FileName { get; set; }
        public int[] X { get; set; }
        public int[] Y { get; set; }
        public int[] Length { get; set; }
        public int[] Breadth { get; set; }
        public int[] Size { get; set; }

        public string Font { get; set; }
        public int FontStyle { get; set; }
    }
}
