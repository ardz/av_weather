using Aveva.Weather.Models.Domain;
using Newtonsoft.Json;

namespace Aveva.Weather.WebApi.Service;

public class WeatherService : IWeatherService
{
    public  WeatherForecast? UpdateForecast()
    {
        const string filePath = "WeatherForecast.json";

        var weatherJson = string.Empty;

        try
        {
            weatherJson = File.ReadAllText(filePath);
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("The JSON file does not exist.");
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error parsing JSON: {e.Message}");
        }

        return JsonConvert.DeserializeObject<WeatherForecast>(weatherJson);
    }
}