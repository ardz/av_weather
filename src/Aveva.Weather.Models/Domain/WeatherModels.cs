using Newtonsoft.Json;

namespace Aveva.Weather.Models.Domain;

public class Coordinate
{
    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("latitude")]
    public double Latitude { get; set; }
}

public class Temperature
{
    [JsonProperty("celsius")]
    public int Celsius { get; set; }

    [JsonProperty("fahrenheit")]
    public double Fahrenheit { get; set; }
}

public class Precipitation
{
    [JsonProperty("mm")]
    public double Millimeters { get; set; }

    [JsonProperty("inches")]
    public double Inches { get; set; }
}

public class WindSpeed
{
    [JsonProperty("mph")]
    public double Mph { get; set; }

    [JsonProperty("kph")]
    public double Kph { get; set; }
}

public class CloudCover
{
    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("cover_percentage")]
    public double? cover_percentage { get; set; } // Made it nullable because of "DATA_NOT_FOUND" example

    [JsonProperty("day_position")]
    public string day_position { get; set; }
}

public class WeatherModels
{
    [JsonProperty("day")]
    public int Day { get; set; }

    [JsonProperty("temperature")]
    public Temperature Temperature { get; set; }

    [JsonProperty("pressure")]
    public int Pressure { get; set; }

    [JsonProperty("precipitation")]
    public Precipitation Precipitation { get; set; }

    [JsonProperty("wind_speed")]
    public WindSpeed wind_speed { get; set; }

    [JsonProperty(nameof(cloud_cover))]
    public List<CloudCover> cloud_cover { get; set; }
}

public class Forecast
{
    [JsonProperty("day_range")]
    public int day_range { get; set; }

    [JsonProperty("weather")]
    public List<WeatherModels> Weather { get; set; }
}

public class Location
{
    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("coordinates")]
    public Coordinate Coordinates { get; set; }

    [JsonProperty("population")]
    public int? Population { get; set; } // Made it nullable because New York example doesn't have it

    [JsonProperty("country")]
    public string Country { get; set; }

    [JsonProperty("forecast")]
    public Forecast Forecast { get; set; }
}

public class WeatherForecast
{
    [JsonProperty("location")]
    public List<Location> Location { get; set; }
}