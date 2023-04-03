using System;
using DynatestSourceGenerator.Abstractions.Attributes;

namespace Demo
{
    [GenerateDto]
    public class Class
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }

    }

}
