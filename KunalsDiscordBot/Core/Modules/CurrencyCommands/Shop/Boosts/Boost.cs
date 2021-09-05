using System;
using System.Linq;
using System.Threading.Tasks;

using DiscordBotDataBase.Dal.Models.Profile.Boosts;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops.Boosts
{
    public enum BoostType
    {
        Luck,
        Invincibility,
        TheftProtection
    }

    public partial class Boost 
    {
        public string Name { get; protected set; }

        public int PercentageIncrease { get; protected set; }
        public TimeSpan TimeSpan { get; protected set; }
        public DateTime Start { get; protected set; }

        protected BoostType BoostType { get; set; }

        public Boost()
        {

        }

        public Boost(string _name)
        {
            Name = _name;
        }

        public Boost(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start)
        {
            Name = _name;
            PercentageIncrease = _percentageIncrease;
            TimeSpan = _span;
            Start = _start;
        }

        public static explicit operator Boost(BoostData data) => CreateFrom(data);

        protected static Boost CreateFrom(BoostData data)
        {
            var boostToGet = AllBoosts.FirstOrDefault(x => x.Name.ToLower() == data.Name.ToLower());
            if (boostToGet == null)
                throw new InvalidCastException();

            return boostToGet.CreateClone(data.Name, data.PercentageIncrease, TimeSpan.Parse(data.TimeSpan), DateTime.Parse(data.StartTime));
        }

        protected virtual Boost CreateClone(string _name, int _percentageIncrease, TimeSpan _span, DateTime _start) => new Boost(_name, _percentageIncrease, _span, _start);

        public virtual Task<bool?> CompareImportance(Boost other)
        {
            if (other.BoostType != BoostType)
                return Task.FromResult<bool?>(null);

            return Task.FromResult<bool?>(other.PercentageIncrease > PercentageIncrease);
        }
    }
}
