using Clays.Application.Common.Interfaces;
using Clays.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Clays.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController(IMachineService machineService) : ControllerBase
    {

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {
            await machineService.SaveMachine(new Machine(){Id = 1, Name = "Machine1"}, CancellationToken.None);
            return Ok();
        }
    }
}
