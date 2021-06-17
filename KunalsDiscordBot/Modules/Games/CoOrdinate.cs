using System;
namespace KunalsDiscordBot.Modules.Games
{
    public class CoOrdinate
    {
        public int x { get; set; }
        public int y { get; set; }
        public int value { get; set; }

        public static bool EvaluatePosition(int x, int y, int maxX, int maxY) => x < maxX && x >= 0 && y < maxY && y >= 0;

        public CoOrdinate GetDownPosition()
        {
            CoOrdinate ordinate = new CoOrdinate();
            ordinate.x = x;
            ordinate.y = y + 1;

            return ordinate;
        }

        public CoOrdinate GetDiagnolDownPosition()
        {
            CoOrdinate ordinate = new CoOrdinate();
            ordinate.x = x + 1;
            ordinate.y = y + 1;

            return ordinate;
        }

        public CoOrdinate GetDiagnolUpPosition()
        {
            CoOrdinate ordinate = new CoOrdinate();
            ordinate.x = x + 1;
            ordinate.y = y - 1;

            return ordinate;
        }

        public CoOrdinate GetRightPosition()
        {
            CoOrdinate ordinate = new CoOrdinate();
            ordinate.x = x + 1;
            ordinate.y = y;

            return ordinate;
        }
    }
}
