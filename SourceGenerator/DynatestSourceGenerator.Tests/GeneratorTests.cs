using System.Text;
using System.Threading.Tasks;
using DynatestSourceGenerator.Tests.Helpers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;

namespace DynatestSourceGenerator.Tests;

using TestMachine = CSharpSourceGeneratorTest<Adapter<Generator>,NUnitVerifier>;

[TestFixture]
public class GeneratorTests
{
    [Test]
    public async Task GeneratorShouldGenerate_ExpectedCode()
    {
        var source = @"

                namespace MyNamespace
                {
                    [GenerateDto]
                    public class MyClass
                    {
                        public int Id { get; set; }
                        public string Name { get; set; }
                    }
                }";

        var expected = @"using System.Dynamic;
using System.Collections;
using SourceDto;
using MyNamespace;

namespace SourceDto
{
    public class MyClassDTO
    {
		public int Id { get; set; }
		public string Name { get; set; }

        public MyClassDTO Map(MyClass instance)
        {
			Id = instance.Id;
			Name = instance.Name;
			return this;
		}
	}
}
";
        var test = new TestMachine
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default,
            TestState =
            {
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS0246").WithSpan(5, 22, 5, 33).WithArguments("GenerateDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(5, 22, 5, 33).WithArguments("GenerateDtoAttribute"),

                },
                GeneratedSources =
                {
                    (typeof(Adapter<Generator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
                },
            }
        };
        // Act & Assert
        await test.RunAsync();
    }
    
    [Test]
    public async Task ExcludePropertyShouldGenerate_ExpectedCodeWithoutName()
    {
        var source = @"

                namespace MyNamespace
                {
                    [GenerateDto]
                    public class MyClass
                    {
                        public int Id { get; set; }
                        [ExcludeProperty]
                        public string Name { get; set; }
                    }
                }";

        var expected = @"using System.Dynamic;
using System.Collections;
using SourceDto;
using MyNamespace;

namespace SourceDto
{
    public class MyClassDTO
    {
		public int Id { get; set; }

        public MyClassDTO Map(MyClass instance)
        {
			Id = instance.Id;
			return this;
		}
	}
}
";
        var test = new TestMachine
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default,
            TestState =
            {
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS0246").WithSpan(5, 22, 5, 33).WithArguments("GenerateDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(5, 22, 5, 33).WithArguments("GenerateDtoAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(9, 26, 9, 41).WithArguments("ExcludeProperty"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(9, 26, 9, 41).WithArguments("ExcludePropertyAttribute")
                },
                GeneratedSources =
                {
                    (typeof(Adapter<Generator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
                },
            }
        };
        
        // Act & Assert
        await test.RunAsync();
    }
    [Test]
    public async Task UseExistingDtoShouldGenerate_ExpectedCodeWithDto()
    {
        var source = @"
namespace MyNamespace
{
    [GenerateDto]
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        [UseExistingDto]
        public MyClass MyClass { get; set; }
    }
}";

        var expected = @"using System.Dynamic;
using System.Collections;
using SourceDto;
using MyNamespace;

namespace SourceDto
{
    public class WeatherForecastDTO
    {
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		
        public MyClassDTO MyClass { get; set; }

        public WeatherForecastDTO Map(WeatherForecast instance)
        {
			Date = instance.Date;
			TemperatureC = instance.TemperatureC;
			MyClass = new MyClassDTO().Map(instance.MyClass);
			return this;
		}
	}
}
";
        var test = new TestMachine
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default,
            TestState =
            {
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS0246").WithSpan(4, 6, 4, 17).WithArguments("GenerateDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(4, 6, 4, 17).WithArguments("GenerateDtoAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(7, 16, 7, 24).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(9, 10, 9, 24).WithArguments("UseExistingDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(9, 10, 9, 24).WithArguments("UseExistingDtoAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(10, 16, 10, 23).WithArguments("MyClass"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 10, 10, 10, 18).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 13, 16, 13, 26).WithArguments("MyClassDTO"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 19, 18, 19, 28).WithArguments("MyClassDTO")
                },
                GeneratedSources =
                {
                    (typeof(Adapter<Generator>),"WeatherForecastDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
                },
                MarkupHandling = MarkupMode.Allow
            }
        };
        
        // Act & Assert
        await test.RunAsync();
    }
    
    [Test]
    public async Task UseExistingDtoWithInputShouldGenerate_ExpectedCodeForTestingWeather()
    {
        var source = 
            """
namespace MyNamespace
{
    [GenerateDto("WeatherForecastDTO", "TestingWeather")]
    public class WeatherForecast
    {
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        [ExcludeProperty("TestingWeather")]
        public string Summary { get; set; }

        [UseExistingDto("TestingWeather > Station2")]
        public Station Station { get; set; }
    }
}
""";

        var expectedTestingWeather = @"using System.Dynamic;
using System.Collections;
using SourceDto;
using MyNamespace;

namespace SourceDto
{
    public class TestingWeather
    {
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		
        public Station2 Station { get; set; }

        public TestingWeather Map(WeatherForecast instance)
        {
			Date = instance.Date;
			TemperatureC = instance.TemperatureC;
			Station = new Station2().Map(instance.Station);
			return this;
		}
	}
}
";
        var expectedDto =
            """
using System.Dynamic;
using System.Collections;
using SourceDto;
using MyNamespace;

namespace SourceDto
{
    public class WeatherForecastDTO
    {
		public DateTime Date { get; set; }
		public int TemperatureC { get; set; }
		[ExcludeProperty("TestingWeather")]
        public string Summary { get; set; }
		
        public StationDTO Station { get; set; }

        public WeatherForecastDTO Map(WeatherForecast instance)
        {
			Date = instance.Date;
			TemperatureC = instance.TemperatureC;
			Summary = instance.Summary;
			Station = new StationDTO().Map(instance.Station);
			return this;
		}
	}
}

""";
        var test = new TestMachine
        {
            TestCode = source,
            ReferenceAssemblies = ReferenceAssemblies.Default,
            TestState =
            {
                ExpectedDiagnostics =
                {
                    DiagnosticResult.CompilerError("CS0246").WithSpan(3, 6, 3, 17).WithArguments("GenerateDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(3, 6, 3, 17).WithArguments("GenerateDtoAttribute"), 
                    DiagnosticResult.CompilerError("CS0246").WithSpan(6, 16, 6, 24).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(8, 10, 8, 25).WithArguments("ExcludeProperty"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(8, 10, 8, 25).WithArguments("ExcludePropertyAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(11, 10, 11, 24).WithArguments("UseExistingDto"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(11, 10, 11, 24).WithArguments("UseExistingDtoAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan(12, 16, 12, 23).WithArguments("Station"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 10, 10, 10, 18).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 13, 16, 13, 24).WithArguments("Station2"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 19, 18, 19, 26).WithArguments("Station2"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 10, 10, 10, 18).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 12, 4, 12, 19).WithArguments("ExcludeProperty"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 12, 4, 12, 19).WithArguments("ExcludePropertyAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 15, 16, 15, 26).WithArguments("StationDTO"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.Generator, DynatestSourceGenerator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 22, 18, 22, 28).WithArguments("StationDTO"),
                },
                GeneratedSources =
                {
                    (typeof(Adapter<Generator>),"WeatherForecastDTO.g.cs",SourceText.From(expectedDto,Encoding.UTF8)),
                    (typeof(Adapter<Generator>),"TestingWeather.g.cs",SourceText.From(expectedTestingWeather,Encoding.UTF8))
                }
            }
        };
        
        // Act & Assert
        await test.RunAsync();
    }
}