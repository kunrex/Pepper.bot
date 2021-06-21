using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace KunalsDiscordBot.Modules.Games.Complex.Battleship
{
    public class Ship
    {
        public List<BattleShipCoOrdinate> ordinates { get; set; }

        public bool isDead { get; private set; }

        public Ship(List<BattleShipCoOrdinate> coOrdinates)
        {
            ordinates = coOrdinates;
        }

        public async Task<(bool, bool)> CheckForHit(BattleShipCoOrdinate ordinateHit)
        {
            var coOrdinate = ordinates.FirstOrDefault(x => x.x == ordinateHit.x && x.y == ordinateHit.y);

            if (coOrdinate != null && coOrdinate != default)
            {
                coOrdinate.type = BattleShipCoOrdinate.OrdinateType.shipHit;

                isDead = await CheckForDead();
                return (true, isDead);
            }

            return (false, false);
        }

        private async Task<bool> CheckForDead()
        {
            foreach (var coOrdinate in ordinates)
                if (coOrdinate.type == BattleShipCoOrdinate.OrdinateType.ship)
                    return false;

            await Task.CompletedTask;
            return true;
        }
    }
}
