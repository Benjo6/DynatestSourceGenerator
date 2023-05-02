using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SourceDto;
using System;

namespace Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var theObject = new Class() {Date = System.DateTime.Now, Forecasts = new []{new WeatherForecast(){Date = DateTime.Now,Station = new Station(){Level = 1,Name = "Hey"},Summary = "Hello",TemperatureC = 30}}};
            var theObjectDTO = new ClassB().MapFrom(theObject);
            Console.WriteLine(theObjectDTO.Date);
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}