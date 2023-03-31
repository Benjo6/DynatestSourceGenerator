using DynatestSourceGenerator.Abstractions.Attributes;

namespace Demo
{
    [GenerateDto("StationDTO", "StationWithNoNameDTO")]
    public class Station
    {
        [ExcludeProperty("StationWithNoNameDTO")]
        public string Name { get; set; }

        public int Level { get; set; }
    }
}