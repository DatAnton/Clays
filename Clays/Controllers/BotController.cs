using Telegram.Bot.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Clays.Application.Common.Interfaces;

namespace Clays.Controllers
{
    [ApiController]
    [Route("api/message/update")]
    [AllowAnonymous]
    public class BotController : Controller
    {
        private string _secretToken;
        private ITelegramBotService _telegramBotService { get; }
        private IUserService _userService { get; }

        public BotController(IConfiguration configuration, ITelegramBotService telegramBotService, 
            IUserService userService)

        {
            _secretToken = configuration.GetValue<string>("BotSecretToken") ?? throw new ArgumentException("Secret token cannot be empty");
            
            _telegramBotService = telegramBotService;
            _userService = userService;
        }


        [HttpPost]
        public async Task<IActionResult> Post(Update update)
        {
            Request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var signature);
            if (string.IsNullOrEmpty(signature) || signature != _secretToken)
            {
                return Unauthorized("Unauthorized Request");
            }

            if(update.Message?.Text == "/start")
            {
                await _userService.CreateUserFromBotAsync(update, CancellationToken.None);   
                await _telegramBotService.SendMessage(update.Message.Chat.Id, "We registered you as a user!");
            }
            else
            {
                await _telegramBotService.SendMessage(update.Message.Chat.Id, "Wrong command!");    
            }
            
            return Ok();
        }
    }
}
