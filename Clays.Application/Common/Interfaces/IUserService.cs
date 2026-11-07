using System;
using Telegram.Bot.Types;

namespace Clays.Application.Common.Interfaces;

public interface IUserService
{
    Task<bool> IsTelegramUserRegisteredAsync(Update update, CancellationToken cancellationToken);
}
