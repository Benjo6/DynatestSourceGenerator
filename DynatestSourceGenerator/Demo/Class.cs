using DynatestSourceGenerator.Abstractions.Attributes;
using System;
using System.Collections.Generic;

namespace Demo
{
    [GenerateDto("ClassB")]
    public class Class
    {
        public DateTime Date { get; set; }
        [UseExistingDto]
        public IEnumerable<WeatherForecast> WeatherForecasts { get; set; }

        [UseExistingDto("TestingWeather")]
        public WeatherForecast[] Forecast { get; set; }


    }
}
