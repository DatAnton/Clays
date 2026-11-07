using Clays.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;

namespace Clays.Infrastructure.Services;

public class UserService(IApplicationDbContext context) : IUserService
{
    public Task<bool> IsTelegramUserRegisteredAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is null)
        {
            return Task.FromResult(false);
        }

        return context.Users.AnyAsync(user => user.ChatId == update.Message.Chat.Id, cancellationToken);
    }
}
