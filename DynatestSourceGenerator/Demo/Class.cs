using DynatestSourceGenerator.Abstractions.Attributes;
using System;
using System.Collections.Immutable;

namespace Demo
{
    [GenerateDto("ClassB")]
    public class Class
    {
        public DateTime Date { get; set; }
        [UseExistingDto]
        public ImmutableArray<WeatherForecast> WeatherForecasts { get; set; }

        [UseExistingDto("TestingWeather")]
        public WeatherForecast[] Forecast { get; set; }


    }
}
