using System;
using static KunalsDiscordBot.Modules.Games.Players.PacmanPlayer;

namespace KunalsDiscordBot.Modules.Games
{
    public partial struct Coordinate
    {
        public static Direction GetDirection(Coordinate ordinate, Coordinate other)
        {
            if (ordinate == other)
                return Direction.up;

            if (ordinate.x > other.x)
                return Direction.left;
            else if (ordinate.x < other.x)
                return Direction.right;

            if (ordinate.y > other.y)
                return Direction.down;
            else if (ordinate.y < other.y)
                return Direction.up;

            return Direction.up;
        }
    }
}
