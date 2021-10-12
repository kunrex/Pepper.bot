using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Cloud.Dialogflow.V2;

using KunalsDiscordBot.Core.Configurations;

using Environment = System.Environment;

namespace KunalsDiscordBot.Core.Chatting
{
    public sealed class Chatbot
    {
        public string ProjectId { get; private set; }

        public Chatbot(PepperConfigurationManager configManager)
        {
            var chatbotConfig = configManager.BotConfig.ChatbotConfig;

            Environment.SetEnvironmentVariable(chatbotConfig.GoogleCreedentialsVariable, chatbotConfig.GoogleCredentialsPath);
            ProjectId = chatbotConfig.ProjectId;
        }

        public async Task<string> GetRepsonse(string sentence)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var client = await SessionsClient.CreateAsync(cancellationTokenSource.Token);

            var dialogFlow = client.DetectIntent(
                new SessionName("pepper-tqfv", "123456789"),
                new QueryInput
                {
                    Text = new TextInput
                    {
                        Text = sentence,
                        LanguageCode = "en-US"
                    }
                }
            );

            if (cancellationTokenSource.IsCancellationRequested)
                return "Time out";

            return dialogFlow.QueryResult.FulfillmentText;
        }
    }
}
