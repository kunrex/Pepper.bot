using System;
using System.Threading.Tasks;
using System.IO;

//D# name spaces
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using KunalsDiscordBot.Attributes;

namespace KunalsDiscordBot.Modules.School
{
    [Group("School")]
    [Decor("Brown", ":school:")]
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class SchoolCommands : BaseCommandModule
    {
        [Command("timetable")]
        [Description("Shows the time table")]
        public async Task TimeTable(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("").ConfigureAwait(false);
        }

        [Command("holiday")]
        [Description("Shows the holiday list")]
        public async Task Holiday(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("").ConfigureAwait(false);
        }

        [Command("link")]
        [Description("Gives the link of the subject entered \n Ex: pepper link bio")]
        public async Task TeachersLink(CommandContext ctx, string subject)
        {
            string fileVal = File.ReadAllText("ImportantLinks.json");
            DataClass data = System.Text.Json.JsonSerializer.Deserialize<DataClass>(fileVal);

            for (int i = 0; i < data.subjects.Length; i++)
            {
                if (data.subjects[i].Equals(subject.ToLower()))
                {
                    await ctx.Channel.SendMessageAsync(data.links[i]).ConfigureAwait(false);
                    return;
                }
            }

            await ctx.Channel.SendMessageAsync("Given subject not found").ConfigureAwait(false);
        }

        /*[Command("Add.HW")]
        [Description("adds an home work, leave no spaces (Suggested: use underscores)")]
        public async Task AddHW(CommandContext ctx, string hw)
        {
            BotInfo.homeWorks.Add(ctx.Member.Nickname + " added " + hw + " on " + DateTime.Now.ToString("dddd, dd MMMM yyyy"));
            await ctx.Channel.SendMessageAsync("Added your homework");
        }

        [Command("Get.HW")]
        [Description("gets homeworks")]
        public async Task GetHomeWork(CommandContext ctx)
        {
            if (BotInfo.homeWorks.Count == 0)//no homeworks
            {
                await ctx.Channel.SendMessageAsync("No homeworks have been added");
                return;
            }

            foreach (string String in BotInfo.homeWorks)
                await ctx.Channel.SendMessageAsync(String);

            await ctx.Channel.SendMessageAsync("\n That is all");
        }

        [Command("CLear.HW")]
        [Description("gets homeworks")]
        public async Task ClearHW(CommandContext ctx)
        {
            BotInfo.homeWorks.Clear();
            await ctx.Channel.SendMessageAsync("Cleared HmeWorks").ConfigureAwait(false);
        }*/

        private class DataClass
        {
            public string[] links { get; set; }
            public string[] subjects { get; set; }
        }
    }
}
