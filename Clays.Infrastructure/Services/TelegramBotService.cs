using Clays.Application.Common.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Clays.Infrastructure.Services
{
    public class TelegramBotService(ITelegramBotClient botClient) : ITelegramBotService
    {
        public async Task SendMessage(ChatId chatId, string text)
        {
            await botClient.SendMessage(chatId, text);
        }

    }
}
