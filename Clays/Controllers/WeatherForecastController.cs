using Clays.Application.Common.Interfaces.Scrapers;
using Microsoft.AspNetCore.Mvc;

namespace Clays.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IKiertonetScraper kiertonetScraper) : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            await kiertonetScraper.Scrape();
            return Ok();
        }
    }
}
