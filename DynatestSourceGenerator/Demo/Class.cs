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
        public List<WeatherForecast> WeatherForecasts { get; set; }

    }

    [GenerateDto]
    public class Tester
    {
        [UseExistingDto]
        public Class Class { get; set; }
    }

}
