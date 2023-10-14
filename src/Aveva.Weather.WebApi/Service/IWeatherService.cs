using Aveva.Weather.Models.Domain;

namespace Aveva.Weather.WebApi.Service;

public interface IWeatherService
{
    WeatherForecast? UpdateForecast();
}