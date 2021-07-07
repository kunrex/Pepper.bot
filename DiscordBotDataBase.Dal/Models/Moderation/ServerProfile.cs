using System;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

namespace DiscordBotDataBase.Dal.Models.Moderation
{
    public class ServerProfile : Entity
    {
        public long GuildId { get; set; }
        public long MutedRoleId { get; set; }

        public List<Rule> Rules  {get; set; } = new List<Rule>();
    }
}
