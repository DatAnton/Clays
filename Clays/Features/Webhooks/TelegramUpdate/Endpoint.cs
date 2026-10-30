using Clays.Application.Common.Interfaces;

namespace Clays.Features.Webhooks.TelegramUpdate;

public sealed class Endpoint : FastEndpoints.Endpoint<Request>
{
    private readonly string _secretToken;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IUserService _userService;

    public Endpoint(IConfiguration configuration, ITelegramBotService telegramBotService, IUserService userService)
    {
        _secretToken = configuration.GetValue<string>("BotSecretToken")
            ?? throw new ArgumentException("Secret token cannot be empty");
        _telegramBotService = telegramBotService;
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/api/message/update");
        AllowAnonymous();
    }

    public override async Task HandleAsync(Request update, CancellationToken cancellationToken)
    {
        if (!HttpContext.Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var signature)
            || string.IsNullOrEmpty(signature) || signature != _secretToken)
        {
            await Send.UnauthorizedAsync(cancellationToken);
            return;
        }
        if (update.Message is null)
        {
            await Send.OkAsync(cancellationToken);
            return;
        }
        if (update.Message.Text == "/start")
        {
            await _userService.CreateUserFromBotAsync(update, cancellationToken);
            await _telegramBotService.SendMessage(update.Message.Chat.Id, "We registered you as a user!");
        }
        else
        {
            await _telegramBotService.SendMessage(update.Message.Chat.Id, "Wrong command!");
        }
        await Send.OkAsync(cancellationToken);
    }
}
