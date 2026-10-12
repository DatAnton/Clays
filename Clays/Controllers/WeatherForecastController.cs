using Microsoft.AspNetCore.Mvc;

namespace Clays.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public IActionResult Get()
        {
            return Ok("it works");
        }
    }
}
