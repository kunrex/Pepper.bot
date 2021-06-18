using System;
using System.Threading.Tasks;
using DiscordBotDataBase.Dal.Models.Items;
using DiscordBotDataBase.Dal.Models.Profile;
using DSharpPlus.Entities;
using System.Collections.Generic;

namespace KunalsDiscordBot.Services.Currency
{
    public interface IProfileService
    {
        public Task<Profile> GetProfile(DiscordMember member, bool sameMember = true);
        public Task<Profile> CreateProfile(DiscordMember member);

        public Task<bool> AddXP(DiscordMember member, int val);
        public Task<bool> ChangeCoins(DiscordMember member, int val);
        public Task<bool> ChangeCoinsBank(DiscordMember member, int val);

        public Task<ItemDBData> GetItem(DiscordMember member, string name);
        public Task<List<ItemDBData>> GetItems(DiscordMember member);
        public Task<bool> AddOrRemoveItem(DiscordMember member, string name, int quantity); 
    }
}
