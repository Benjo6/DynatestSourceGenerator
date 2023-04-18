using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SourceDto;

namespace Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var classB = new ClassB() { Date = DateTime.Now,WeatherForecasts = new List<WeatherForecastDTO>() ,Forecast = Array.Empty<TestingWeather>() }; 
            var idea = classB.MapTo();
            Console.WriteLine(idea.ToString());
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}