using Telegram.Bot.Types;

namespace Clays.Application.Common.Interfaces
{
    public interface ITelegramBotService
    {
        Task SendMessage(ChatId chatId, string text);
    }
}
