using Telegram.Bot.Types;
using Microsoft.AspNetCore.Mvc;
using Clays.Application.Common.Interfaces;

namespace Clays.Controllers
{
    [ApiController]
    [Route("api/message/update")]
    public class BotController : Controller
    {
        private string _secretToken;
        private ITelegramBotService _telegramBotService { get; }

        public BotController(IConfiguration configuration, ITelegramBotService telegramBotService)
        {
            _secretToken = configuration.GetValue<string>("BotSecretToken") ?? throw new ArgumentException("Secret token cannot be empty");
            
            _telegramBotService = telegramBotService;
        }


        [HttpPost]
        public async Task<IActionResult> Post(Update update)
        {
            Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var signature);
            if (string.IsNullOrEmpty(signature) || signature != _secretToken)
            {
                return Unauthorized("Unauthorized Request");
            }

            await _telegramBotService.SendMessage(update.Message.Chat.Id, "Message received2!");
            return Ok();
        }
    }
}
