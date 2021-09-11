using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;

using KunalsDiscordBot.Core.DialogueHandlers.Steps;
using KunalsDiscordBot.Core.Modules.CurrencyCommands.Shops;

namespace KunalsDiscordBot.Core.DialogueHandlers
{
    public class DialogueHandler
    {
        private DialogueHandlerConfig Configuration { get; set; }

        private List<Step> Steps { get; set; } = new List<Step>();

        private int stepIndex;
        private bool started;

        public DialogueHandler(DialogueHandlerConfig config)
        {
            Configuration = config;

            stepIndex = 0;
        }

        public DialogueHandler WithSteps(List<Step> newSteps)
        {
            Steps.AddRange(newSteps);
            if (Configuration.QuickStart)
                Task.Run(() => ProcessDialogue());

            return this;
        }

        public async Task<List<DialougeResult>> ProcessDialogue()
        {
            if (started)
                return null;

            started = true;
            List<DialougeResult> results = new List<DialougeResult>();
            DialougeResult previousResult = default;

            while (stepIndex < Steps.Count)
            {
                Step currentStep = Steps[stepIndex];
                previousResult = await currentStep.ProcessStep(Configuration.Channel, Configuration.Member, Configuration.Client, Configuration.UseEmbed, previousResult);

                if (!previousResult.WasCompleted && Configuration.RequireFullCompletion)
                    return results;

                stepIndex++;
                results.Add(previousResult);
            }

            return results;
        }
    }
}
