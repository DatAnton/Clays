using Clays.Application.Common.Interfaces.Scrapers;
using Clays.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Clays.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IKiertonetScraper scraper, IApplicationDbContext context) : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            return Ok(new { text = "It works" });
        }
    }
}
