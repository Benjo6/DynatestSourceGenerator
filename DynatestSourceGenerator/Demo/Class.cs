using DynatestSourceGenerator.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Demo
{
    [GenerateDto("ClassB")]
    public class Class
    {
        public DateTime Date { get; set; }
        
        // Dictionary
        [UseExistingDto ("TestingWeather")]
        public IDictionary<int,WeatherForecast> WeatherForecasts { get; set; }

        
        // Array
        [UseExistingDto("TestingWeather")]
        public WeatherForecast[] Forecasts { get; set; }
        
        //List
        [UseExistingDto ("TestingWeather")]
        public List<WeatherForecast> Fore { get; set; }
        

        [UseExistingDto ("TestingWeather")]
        public string Cla { get; set; }
        
        [UseExistingDto ("TestingWeather")]
        public WeatherForecast Clay { get; set; }
    }
}
