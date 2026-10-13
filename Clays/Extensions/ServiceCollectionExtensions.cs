using Telegram.Bot;

namespace Clays.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTelegramBotClient(this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            var botToken = configuration.GetValue<string>("BotToken");

            var botBaseUrl = configuration.GetValue<string>("BotBaseUrl");

            var secretToken = configuration.GetValue<string>("BotSecretToken");

            if (string.IsNullOrEmpty(botToken) || string.IsNullOrEmpty(botBaseUrl)
                || string.IsNullOrEmpty(secretToken))
            {
                throw new ArgumentException("Bot credentials are empty");
            }
            var client = new TelegramBotClient(botToken);
            var webHook = $"{botBaseUrl}/api/message/update";
            client.SetWebhook(webHook, secretToken: secretToken);

            return serviceCollection
                .AddTransient<ITelegramBotClient>(_ => client);
        }
    }
}
