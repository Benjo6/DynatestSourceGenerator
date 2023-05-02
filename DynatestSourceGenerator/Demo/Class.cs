using DynatestSourceGenerator.Abstractions.Attributes;
using System;
using System.Collections.Generic;

namespace Demo
{
    [GenerateDto("ClassB")]
    public class Class
    {
        public DateTime Date { get; set; }

        // Array
        [UseExistingDto("TestingWeather")]
        public WeatherForecast[] Forecasts { get; set; }

    }


}

