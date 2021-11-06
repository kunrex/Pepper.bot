using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Profile;
using KunalsDiscordBot.Services.Currency;

namespace KunalsDiscordBot.Core.Modules.CurrencyCommands.Models.Items
{
    public class AnimalItem : DecoritiveItem
    {
        public bool IsRareItem { get; private set; }
        private string[] RewriteSentences { get; set; }

        public AnimalItem(string name, string description, string icon = ":grey_question:") : base(name, -1, description, UseType.Decoration | UseType.NonBuyable, icon)
        {

        }

        public AnimalItem(string name, string description, string icon = ":grey_question:", params string[] sentences) : base(name, -1, description, UseType.Decoration | UseType.NonBuyable, icon)
        {
            IsRareItem = true;
            RewriteSentences = sentences;
        }

        public override Task<UseResult> Use(Profile profile, IProfileService profileService) => Task.FromResult(new UseResult
        {
            UseComplete = false,
            Message = "You can't use this item?"
        });

        public string GetSentence() => IsRareItem ? RewriteSentences[new Random().Next(0, RewriteSentences.Length)] : throw new InvalidOperationException(); 
    }
}
