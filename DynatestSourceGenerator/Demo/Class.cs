using DynatestSourceGenerator.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using SourceDto;

namespace Demo
{
    [GenerateDto("ClassB")]
    public class Class
    {
        public DateTime Date { get; set; }
        [UseExistingDto] 
        public List<WeatherForecast> WeatherForecasts { get; set; }

        [UseExistingDto] 
        public WeatherForecast[] Forecast { get; set; }


    }
}
