using System;
using DynatestSourceGenerator.Attributes;

namespace Demo
{
    [GenerateDto("WeatherForecastDTO", "TestingWeather")]
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }

        public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);

        public double TemperatureK { get; } = 0 + 273.15;

        [ExcludeProperty("TestingWeather")]
        public string Summary { get; set; }

        [UseExistingDto("TestingWeather > StationWithNoNameDTO")]
        public Station Station { get; set; }
    }
}