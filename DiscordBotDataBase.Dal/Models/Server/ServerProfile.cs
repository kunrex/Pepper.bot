using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DiscordBotDataBase.Dal.Models.Servers.Models;

namespace DiscordBotDataBase.Dal.Models.Servers
{
    public class ServerProfile : Entity<long>
    {
        public int RestrictPermissionsToAdmin { get; set; } = 1;
        public int LogErrors { get; set; } = 1;
        public int LogNewMembers { get; set; } = 1;

        public long RulesChannelId { get; set; } = 0;
        public long WelcomeChannel { get; set; } = 0;

        public ModerationData ModerationData { get; set; }      
        public FunData FunData { get; set; }
        public MusicData MusicData { get; set; }
        public GameData GameData { get; set; }
        public AIChatData ChatData { get; set; }
    }
}
