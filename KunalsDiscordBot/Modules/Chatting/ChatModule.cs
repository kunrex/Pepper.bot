using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using KunalsDiscordBot.Core.Modules;
using KunalsDiscordBot.Core.Attributes;
using KunalsDiscordBot.Core.Exceptions;
using KunalsDiscordBot.Services.Modules;
using KunalsDiscordBot.Services.General;
using KunalsDiscordBot.Core.Configurations.Enums;
using KunalsDiscordBot.Core.Configurations.Attributes;

namespace KunalsDiscordBot.Modules.Chatting
{
    [Group("AIChat")]
    [Decor("LightGray", ":robot:"), Aliases("Chat")]
    [Description("Commands to configure the AI chat module")]
    [ModuleLifespan(ModuleLifespan.Transient), ConfigData(ConfigValueSet.AIChat)]
    [RequireBotPermissions(Permissions.SendMessages | Permissions.AccessChannels)]
    public sealed class ChatModule : PepperCommandModule
    {
        public override PepperCommandModuleInfo ModuleInfo { get; protected set; }

        private readonly IServerService serverService;

        public ChatModule(IModuleService moduleService, IServerService _serverService)
        {
            ModuleInfo = moduleService.ModuleInfo[ConfigValueSet.AIChat];
            serverService = _serverService;
        }

        public async override Task BeforeExecutionAsync(CommandContext ctx)
        {
            var configPermsCheck = ctx.Command.CustomAttributes.FirstOrDefault(x => x is CheckConfigigurationPermissionsAttribute) != null;

            if (configPermsCheck)
            {
                var profile = await serverService.GetServerProfile(ctx.Guild.Id).ConfigureAwait(false);

                if (profile.RestrictPermissionsToAdmin == 1 && (ctx.Member.PermissionsIn(ctx.Channel) & Permissions.Administrator) != Permissions.Administrator)
                {
                    await ctx.RespondAsync(":x: You need to be an admin to run this command").ConfigureAwait(false);
                    throw new CustomCommandException();
                }
            }
        }

        [Command("ToggleAIChat")]
        [RequireUserPermissions(Permissions.Administrator)]
        [Description("Enables or Disables AI chatting in the server"), ConfigData(ConfigValue.AIChatEnabled)]
        public async Task AIChatChannel(CommandContext ctx, bool toSet)
        {
            await serverService.ModifyData(await serverService.GetChatData(ctx.Guild.Id), x => x.Enabled = toSet ? 1 : 0).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Changed `AIChatEnabled` to {toSet}",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }

        [Command("SetAIChatChannel")]
        [CheckConfigigurationPermissions]
        [Description("Sets the channel used for AI chatting"), ConfigData(ConfigValue.AIChatEnabled)]
        public async Task AIChatChannel(CommandContext ctx, DiscordChannel channel)
        {
            var profile = await serverService.GetChatData(ctx.Guild.Id);
            if(profile.Enabled == 0)
            {
                await ctx.RespondAsync(new DiscordEmbedBuilder
                {
                    Description = "AI chatting must be enabled to run this command. You can do so by running the `aichat toggleaichat` command",
                    Color = ModuleInfo.Color
                });
            }

            await serverService.ModifyData(await serverService.GetChatData(ctx.Guild.Id), x => x.AIChatChannelID = (long)channel.Id).ConfigureAwait(false);

            await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder
            {
                Title = "Edited Configuration",
                Description = $"Saved {channel.Mention} as the AI Chat channel for the guild: `{ctx.Guild.Name}`",
                Color = ModuleInfo.Color
            }.WithFooter($"User: {ctx.Member.DisplayName}, at {DateTime.Now}")).ConfigureAwait(false);
        }
    }
}
