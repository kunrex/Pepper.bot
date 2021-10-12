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
        public ChatbotConfig Config { get; private set; }

        public Chatbot(PepperConfigurationManager configManager)
        {
            Config = configManager.BotConfig.ChatbotConfig;

            Environment.SetEnvironmentVariable(Config.GoogleCreedentialsVariable, Config.GoogleCredentialsPath);
        }

        public async Task<string> GetRepsonse(string sentence)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(Config.TimeOutime));
            var client = await SessionsClient.CreateAsync(cancellationTokenSource.Token);

            var dialogFlow = client.DetectIntent(
                new SessionName(Config.ProjectId, "123456789"),
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

            var messages = dialogFlow.QueryResult.FulfillmentMessages;
            return messages[new Random().Next(0, messages.Count)].Text.Text_[0];
        }
    }
}
