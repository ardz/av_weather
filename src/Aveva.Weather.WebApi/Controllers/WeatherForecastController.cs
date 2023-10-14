using Aveva.Weather.Models.Domain;
using Aveva.Weather.WebApi.Service;
using Microsoft.AspNetCore.Mvc;

namespace Aveva.Weather.WebApi.Controllers;

[ApiController]
[Route("api/weather")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IWeatherService _weatherService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IWeatherService weatherService)
    {
        _logger = logger;
        _weatherService = weatherService;
    }
    
    [HttpGet("Forecasts")]
    public ActionResult<WeatherForecast> Forecasts()
    {
        var forecast = _weatherService.UpdateForecast();
        
        return Ok(forecast);
    }
}