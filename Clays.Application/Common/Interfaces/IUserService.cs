using System;
using Telegram.Bot.Types;

namespace Clays.Application.Common.Interfaces;

public interface IUserService
{
    Task CreateUserFromBotAsync(Update update, CancellationToken cancellationToken);
}
