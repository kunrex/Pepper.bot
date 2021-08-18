using System.Threading.Tasks;
using System.Collections.Generic;

namespace KunalsDiscordBot.Core.Modules.GameCommands.Battleship
{
    public class Ship
    {
        public List<Coordinate> ordinates { get; set; }

        public bool isDead { get; private set; }

        public Ship(List<Coordinate> coOrdinates)
        {
            ordinates = coOrdinates;
        }

        public async Task<(bool, bool)> CheckForHit(Coordinate ordinateHit)
        {
            for(int i =0;i< ordinates.Count;i++)
            {
                var ordinate = ordinates[i];

                if(ordinate == ordinateHit)
                {
                    ordinate.type = Coordinate.OrdinateType.shipHit;
                    ordinates[i] = ordinate;

                    isDead = await CheckForDead();
                    return (true, isDead);
                }
            }

            return (false, false);
        }

        private async Task<bool> CheckForDead()
        {
            foreach (var coOrdinate in ordinates)
                if (coOrdinate.type == Coordinate.OrdinateType.ship)
                    return false;

            await Task.CompletedTask;
            return true;
        }
    }
}
