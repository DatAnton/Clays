using Clays.Application.Common.Interfaces;
using Telegram.Bot.Types;

namespace Clays.Infrastructure.Services;

public class UserService(IApplicationDbContext context) : IUserService
{
    public Task CreateUserFromBotAsync(Update update, CancellationToken cancellationToken)
    {
        var existedUser = context.Users.FirstOrDefault(u => u.ChatId == update.Message.Chat.Id);
        if (existedUser is not null)
        {
            return Task.CompletedTask;
        }

        var newUser = new Domain.Entities.User
        {
            ChatId = update.Message.Chat.Id,
            FirstName = update.Message.Chat.FirstName,
            LastName = update.Message.Chat.LastName,
            UserName = update.Message.Chat.Username
        };
        context.Users.Add(newUser);
        return context.SaveChangesAsync(cancellationToken);
    }
}
