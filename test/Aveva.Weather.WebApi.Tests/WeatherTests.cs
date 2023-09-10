using Aveva.Weather.Models.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit.Abstractions;

namespace Aveva.Weather.WebApi.Tests;

public class WeatherTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly HttpClient _client;
    private WeatherForecast? _weatherForecast;

    private async Task WeatherUpdater()
    {
        var response = await _client.GetAsync("/api/weather/Forecasts");

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var settings = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver()
        };

        _weatherForecast = JsonConvert.DeserializeObject<WeatherForecast>(content, settings);
    }

    private WeatherModels? WeatherForCityOnDay(string city, int day)
    {
        return _weatherForecast?.Location
            .FirstOrDefault(l => l.City == city)?.Forecast.Weather
            .FirstOrDefault(w => w.Day == day);
    }

    private static bool IsWithinTenPercentOfTwentyFour(double temperature)
    {
        const double tenPercentValue = 24 * 0.10; // 10% of 24
        const double lowerBound = 24 - tenPercentValue;
        const double upperBound = 24 + tenPercentValue;

        return temperature is >= lowerBound and <= upperBound;
    }

    public WeatherTests(WebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _client = factory.CreateClient();
    }
    
    // "There is no rainfall for day 1 in London"
    [Fact]
    public async Task WhenFirstDayInLondon_ExpectNoRainfall()
    {
        await WeatherUpdater();

        var precipitationValues1 = _weatherForecast?.Location.FirstOrDefault(p => p.City == "London")
            ?.Forecast.Weather.FirstOrDefault()
            ?.Precipitation;

        if (precipitationValues1 != null)
        {
            Assert.Equal(0, precipitationValues1.Inches);
            Assert.Equal(0, precipitationValues1.Millimeters);
        }
        
        // or you could do this
        var precipitationValues2 = WeatherForCityOnDay("London", 1);
        
        if (precipitationValues2 != null)
        {
            Assert.Equal(0, precipitationValues2.Precipitation.Inches);
            Assert.Equal(0, precipitationValues2.Precipitation.Millimeters);
        }
    }

    // "There are the correct number of entries for London"
    // this requirement is ambiguous
    // in the real world, wouldn't work on this until it was clarified what it means 
    [Fact]
    public async Task Expect_CorrectNumberOfEntriesForLondon()
    {
        await WeatherUpdater();

        // one possibility, if the requirement meant "there should be 7 days of weather forecasts for London"
        var londonWeatherWeek = _weatherForecast?.Location
            .FirstOrDefault(p => p.City == "London")?.Forecast.Weather;

        Assert.Equal(7, londonWeatherWeek?.Count);
        
        // problem with this above is that it's only checking the number of entries in the collection, not
        // the actual day value itself. You could have 7 items in the list but
        // one of the day values could be "8" for example and the rest 1 to 6 (if we're assuming 
        // 1 to 7 is Sunday to Saturday)
        
    }

    // "We do not know the population for New York"
    [Fact]
    public async Task Expect_UnknownPopulationForNewYork()
    {
        await WeatherUpdater();

        var city = _weatherForecast?.Location.FirstOrDefault(p => p.City == "New York");

        Assert.True(city is { Population: null }, "should be null?");
    }

    // "The pressure rose in New York from day 1"
    [Fact]
    public async Task Expect_IncreasingPressureFromDay1NewYork()
    {
        await WeatherUpdater();

        var weatherNewYork = _weatherForecast?.Location.FirstOrDefault(p => p.City == "New York")?.Forecast;

        var day1Pressure = weatherNewYork?.Weather.FirstOrDefault(d => d.Day == 1)?.Pressure;
        var day2Pressure = weatherNewYork?.Weather.FirstOrDefault(d => d.Day == 2)?.Pressure;

        Assert.True(day2Pressure > day1Pressure, $"{day2Pressure} is not greater than {day1Pressure}");

        // or loop and check the next value is greater than the previous?
        var pressures = new List<int?>
        {
            day1Pressure,
            day2Pressure,
            // 499, would cause test to fail for example
        };
        
        for (var i = 1; i < pressures.Count; i++)
        {
            var previous = pressures[i - 1];
            var current = pressures[i];
        
            if (current > previous)
            {
                _testOutputHelper.WriteLine(
                    $"{current} is greater than previous value ({previous}).");
            }
        
            else
            {
                Assert.Fail($"{current} is not greater than previous value ({previous}).");
            }
        }
    }

    // "The rainfall in London is only ever over half an inch twice"
    [Fact]
    public async Task Expect_RainfallLondonGreaterThanHalfAnInchTwice()
    {
        await WeatherUpdater();

        var dailyRainfallInches = _weatherForecast?.Location
            .FirstOrDefault(p => p.City == "London")?.Forecast.Weather
            .Select(p => p.Precipitation.Inches);

        var countOverPointFive = dailyRainfallInches?.Count(v => v > 0.50);

        Assert.Equal(2, countOverPointFive);
    }

    // "London will be overcast for the whole of day 5"
    
    // Another ambiguous requirement here, the cover_percentage value is 99.8% overcast, which to me suggests that
    // 0.2% isn't overcast on that day... so is it by definition "not entirely overcast?" what exactly are the criteria
    // by which we can say a day is "overcast"?
    [Fact]
    public async Task Expect_OvercastLondonDay5()
    {
        await WeatherUpdater();

        var cloudCover = WeatherForCityOnDay("London", 5)?.cloud_cover.FirstOrDefault();

        if (cloudCover is { Description: "Overcast", day_position: "All day" })
        {
            Assert.True(cloudCover.cover_percentage >= 100.0);
        }
        
        // or (if you assume that the any "cover_percentage" value counts as "overcast")
        Assert.Equal("Overcast", cloudCover?.Description);
        Assert.Equal("All day", cloudCover?.day_position);
    }

    // "The temperature in New York is always within 10% of 24 degrees Celsius"
    [Fact]
    public async Task Expect_TemperatureNewYorkAlwaysWithin10PercentOf24DegreesCelsius()
    {
        await WeatherUpdater();

        var temperatures = _weatherForecast?.Location.FirstOrDefault(p => p.City == "New York")
            ?.Forecast.Weather.Select(t => t.Temperature).Select(c => c.Celsius);

        if (temperatures != null)

            foreach (var temperature in temperatures.Where(temperature => !IsWithinTenPercentOfTwentyFour(temperature)))
            {
                Assert.Fail($"{temperature} is not within 10% of 24 degrees.");
            }
    }
}