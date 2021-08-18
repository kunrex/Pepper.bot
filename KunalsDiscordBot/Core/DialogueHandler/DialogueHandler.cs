using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public class DialogueHandler
    {
        public string MainTitle { get; private set; }
        public bool UseEmbed { get; private set; }

        public DiscordChannel Channel { get; private set; }
        public DiscordMember Member { get; private set; }
        public DiscordClient Client { get; private set; }

        private List<Step> Steps { get; set; } = new List<Step>();
        private int stepIndex;

        public DialogueHandler(DialogueHandlerConfig config, List<Step> steps)
        {
            MainTitle = config.MainTitle;
            UseEmbed = config.UseEmbed;

            Channel = config.Channel;
            Member = config.Member;
            Client = config.Client;

            Steps.AddRange(steps);
            stepIndex = 0;
        }

        public void AddSteps(List<Step> newSteps) => Steps.AddRange(newSteps);

        public async Task<bool> ProcessDialogue()
        {
            if(MainTitle != null && MainTitle != string.Empty)
                await Channel.SendMessageAsync(MainTitle).ConfigureAwait(false);

            while (stepIndex < Steps.Count)
            {
                Step currentStep = Steps[stepIndex];
                bool completed = await currentStep.ProcessStep(Channel, Member, Client, UseEmbed);

                if (!completed)
                    return false;

                stepIndex++;
            }

            return true;
        }
    }
}
