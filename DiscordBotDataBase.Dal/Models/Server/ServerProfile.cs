using System;
using System.Collections.Generic;
using DiscordBotDataBase.Dal.Models.Moderation.SubData;

namespace DiscordBotDataBase.Dal.Models.Servers
{
    public class ServerProfile : Entity
    {
        public long GuildId { get; set; }

        public long MutedRoleId { get; set; }
        public List<Rule> Rules  {get; set; } = new List<Rule>();

        public int RestrictPermissionsToAdmin { get; set; } = 1;

        public int AllowNSFW { get; set; } = 0;

        public int UseDJRoleEnforcement { get; set; } = 0;
        public long DJRoleId { get; set; }
    }
}
