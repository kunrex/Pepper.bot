using System;
namespace KunalsDiscordBot.Core.Modules.GameCommands
{
    public partial struct Coordinate
    {
        public int x { get; set; }
        public int y { get; set; }
        public int value { get; set; }

        public static bool EvaluatePosition(int x, int y, int maxX, int maxY) => x < maxX && x >= 0 && y < maxY && y >= 0;

        public Coordinate GetDownPosition()
        {
            Coordinate ordinate = new Coordinate();
            ordinate.x = x;
            ordinate.y = y + 1;

            return ordinate;
        }

        public Coordinate GetDiagnolDownPosition()
        {
            Coordinate ordinate = new Coordinate();
            ordinate.x = x + 1;
            ordinate.y = y + 1;

            return ordinate;
        }

        public Coordinate GetDiagnolUpPosition()
        {
            Coordinate ordinate = new Coordinate();
            ordinate.x = x + 1;
            ordinate.y = y - 1;

            return ordinate;
        }

        public Coordinate GetRightPosition()
        {
            Coordinate ordinate = new Coordinate();
            ordinate.x = x + 1;
            ordinate.y = y;

            return ordinate;
        }

        public static bool operator == (Coordinate a, Coordinate b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Coordinate a, Coordinate b) => a.x != b.x || a.y != b.y;

        public static Coordinate operator + (Coordinate a, Coordinate b) => new Coordinate { x = a.x + b.x, y = a.y + b.y };
        public static Coordinate operator - (Coordinate a, Coordinate b) => new Coordinate { x = a.x - b.x, y = a.y - b.y };
    }
}
