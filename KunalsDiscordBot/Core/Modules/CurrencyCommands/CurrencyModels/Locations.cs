using System;
using System.Linq;
using System.Collections.Generic;

using KunalsDiscordBot.Extensions;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models
{
    public partial struct CurrencyModel
    {
        private static readonly CurrencyModel Bank = new CurrencyModel("Bank", 100, 300,
            "You succesfully robbed the bank and got {} coins. Now the police is coming, run!", "You tried robbing the bank but slipped on some money and fell");

        private static readonly CurrencyModel Basement = new CurrencyModel("Basement", 50, 225,
            "You were searching through all your stuff in the basement and found {} coins", "While you were searching the basement, you saw a rat, got scared and bumped your head on a wall");

        private static readonly CurrencyModel Tree = new CurrencyModel("Tree", 50, 225,
            "You found {} coins growing on a branch. I guess money does grown on tress", "While climbing the tree, you slipped and fell.");

        private static readonly CurrencyModel Banana = new CurrencyModel("Banana", 50, 150,
            "While peeling the banana you found {} coins. I wonder how that got there :thonk:", "You choked on the banana and died");

        private static readonly CurrencyModel Discord = new CurrencyModel("Discord", 300, 500,
            "You searched through your DM's to find {} coins", "Someone hacked your account and bough nitro for a lifetime using your money.");

        private static readonly CurrencyModel Road = new CurrencyModel("Road", 100, 200,
            "You stood in the middle of the road, somehow didn't get hit and found {} coins", "You got hit by a car");

        private static readonly CurrencyModel Reddit = new CurrencyModel("Reddit", 150, 200,
           "Your browsed through some subreddits and found {} coins", "Your Wifi sucks and you couldn't browse through reddit");

        private static readonly CurrencyModel Ocean = new CurrencyModel("Ocean", 25, 320,
           "You were swimming in the ocean and found {} coins in a bottle", "You got eaten by a shark");

        private static readonly CurrencyModel Doghouse = new CurrencyModel("Doghouse", 100, 200,
          "You found {} coins in some dog poop you stepped on", "Your dog doesn't like you and bit you");

        public static readonly List<CurrencyModel> Locations = new List<CurrencyModel>() { Bank, Basement, Tree, Banana, Discord, Road, Reddit, Ocean, Doghouse };

        public static IEnumerable<CurrencyModel> Random3Locations = new List<CurrencyModel>(Locations).Shuffle().Take(3);
    }
}
