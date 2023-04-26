using DynatestSourceGenerator.DataTransferObject;
using DynatestSourceGenerator.Tests.Helpers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Threading.Tasks;

namespace DynatestSourceGenerator.Tests;

using TestMachine = CSharpSourceGeneratorTest<Adapter<DataObjectGenerator>, NUnitVerifier>;

[TestFixture]
public class GeneratorTests
{
    [Test]
    public async Task GeneratorShouldGenerate_ExpectedCode()
    {
        var source = @"

                namespace MyNamespace
                {
                    [GenerateDto()]
                    public class MyClass
                    {
                        public int Id { get; set; }
                        public string Name { get; set; }
                    }
                }";

        var expected = @"using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class MyClassDTO
{
	public int Id { get; set; }
	public string Name { get; set; }

    /// <summary>
    /// Maps a <see cref=""MyClass""/> instance to a <see cref=""MyClassDTO""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""MyClass""/> instance to map.</param>
    /// <returns>The mapped <see cref=""MyClassDTO""/> instance.</returns>
    public MyClassDTO MapFrom(MyClass instance)
    {
		if (instance is null)
			return default;
		var target = new MyClassDTO();
		target.Id = instance.Id;
		target.Name = instance.Name;
		return target;
	}

    /// <summary>
    /// Maps a <see cref=""MyClassDTO""/> instance to a <see cref=""MyClass""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""MyClass""/> instance.</returns>
    public MyClass MapTo()
    {
		var target = new MyClass();
		target.Id = Id;
		target.Name = Name;
		return target;
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
                    (typeof(Adapter<DataObjectGenerator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
                },
            }
        };
        // Act & Assert
        await test.RunAsync();
    }

    [Test]
    public async Task GeneratorShouldGenerate_EmptyDto()
    {
        var source = @"

                namespace MyNamespace
                {
                    [GenerateDto()]
                    public class MyClass
                    {

                    }
                }";

        var expected = @"using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class MyClassDTO
{
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
                    DiagnosticResult.CompilerError("CS0246").WithSpan(5, 22, 5, 33).WithArguments("GenerateDtoAttribute")

                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
                },
            }
        };
        // Act & Assert
        await test.RunAsync();
    }

    [Test]
    public async Task GeneratorShouldCollectionsGenerate_EmptyDto()
    {
        var source = @"

                namespace MyNamespace;
                   [GenerateDto()]
    public class MyClass
    {
        [UseExistingDto(""TestingWeather"")]
        public Dictionary<int, WeatherForecast> WeatherForecasts { get; set; }
        [UseExistingDto(""TestingWeather"")]
    	public List<TestingWeather> IWeatherForecasts { get; set; }
}
                   }";

        var expected = @"using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class MyClassDTO
{
	public Dictionary<int, TestingWeather> WeatherForecasts { get; set; }
	public List<TestingWeather> IWeatherForecasts { get; set; }

    /// <summary>
    /// Maps a <see cref=""MyClass""/> instance to a <see cref=""MyClassDTO""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""MyClass""/> instance to map.</param>
    /// <returns>The mapped <see cref=""MyClassDTO""/> instance.</returns>
    public MyClassDTO MapFrom(MyClass instance)
    {
		if (instance is null)
			return default;
		var target = new MyClassDTO();
		if (instance.WeatherForecasts != null)
		{
			target.WeatherForecasts = TestingWeather.MapFromDictionary(instance.WeatherForecasts);
		}
		if (instance.IWeatherForecasts != null)
		{
			target.IWeatherForecasts = TestingWeather.MapFromList(instance.IWeatherForecasts);
		}
		return target;
	}

    /// <summary>
    /// Maps a <see cref=""MyClassDTO""/> instance to a <see cref=""MyClass""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""MyClass""/> instance.</returns>
    public MyClass MapTo()
    {
		var target = new MyClass();
		if (WeatherForecasts != null)
		{
			target.WeatherForecasts = TestingWeather.MapToDictionary(WeatherForecasts);
		}
		if (IWeatherForecasts != null)
		{
			target.IWeatherForecasts = TestingWeather.MapToList(IWeatherForecasts);
		}
		return target;
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
                { // /0/Test0.cs(4,21): error CS0246: The type or namespace name 'GenerateDto' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 21, 4, 32).WithArguments("GenerateDto"),
// /0/Test0.cs(4,21): error CS0246: The type or namespace name 'GenerateDtoAttribute' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 21, 4, 32).WithArguments("GenerateDtoAttribute"),
// /0/Test0.cs(7,10): error CS0246: The type or namespace name 'UseExistingDto' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 10, 7, 24).WithArguments("UseExistingDto"),
// /0/Test0.cs(7,10): error CS0246: The type or namespace name 'UseExistingDtoAttribute' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 10, 7, 24).WithArguments("UseExistingDtoAttribute"),
// /0/Test0.cs(8,16): error CS0246: The type or namespace name 'Dictionary<,>' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(8, 16, 8, 48).WithArguments("Dictionary<,>"),
// /0/Test0.cs(8,32): error CS0246: The type or namespace name 'WeatherForecast' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(8, 32, 8, 47).WithArguments("WeatherForecast"),
// /0/Test0.cs(9,10): error CS0246: The type or namespace name 'UseExistingDto' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(9, 10, 9, 24).WithArguments("UseExistingDto"),
// /0/Test0.cs(9,10): error CS0246: The type or namespace name 'UseExistingDtoAttribute' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(9, 10, 9, 24).WithArguments("UseExistingDtoAttribute"),
// /0/Test0.cs(10,13): error CS0246: The type or namespace name 'List<>' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(10, 13, 10, 33).WithArguments("List<>"),
// /0/Test0.cs(10,18): error CS0246: The type or namespace name 'TestingWeather' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan(10, 18, 10, 32).WithArguments("TestingWeather"),
// /0/Test0.cs(12,20): error CS1022: Type or namespace definition, or end-of-file expected
DiagnosticResult.CompilerError("CS1022").WithSpan(12, 20, 12, 21),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(10,25): error CS0246: The type or namespace name 'TestingWeather' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 10, 25, 10, 39).WithArguments("TestingWeather"),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(11,14): error CS0246: The type or namespace name 'TestingWeather' could not be found (are you missing a using directive or an assembly reference?)
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 11, 14, 11, 28).WithArguments("TestingWeather"),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(25,30): error CS0103: The name 'TestingWeather' does not exist in the current context
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 25, 30, 25, 44).WithArguments("TestingWeather"),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(29,31): error CS0103: The name 'TestingWeather' does not exist in the current context
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 29, 31, 29, 45).WithArguments("TestingWeather"),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(43,30): error CS0103: The name 'TestingWeather' does not exist in the current context
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 43, 30, 43, 44).WithArguments("TestingWeather"),
// DynatestSourceGenerator.Tests\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\MyClassDTO.g.cs(47,31): error CS0103: The name 'TestingWeather' does not exist in the current context
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\MyClassDTO.g.cs", 47, 31, 47, 45).WithArguments("TestingWeather"),

                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
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

        var expected = @"using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class MyClassDTO
{
	public int Id { get; set; }

    /// <summary>
    /// Maps a <see cref=""MyClass""/> instance to a <see cref=""MyClassDTO""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""MyClass""/> instance to map.</param>
    /// <returns>The mapped <see cref=""MyClassDTO""/> instance.</returns>
    public MyClassDTO MapFrom(MyClass instance)
    {
		if (instance is null)
			return default;
		var target = new MyClassDTO();
		target.Id = instance.Id;
		return target;
	}

    /// <summary>
    /// Maps a <see cref=""MyClassDTO""/> instance to a <see cref=""MyClass""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""MyClass""/> instance.</returns>
    public MyClass MapTo()
    {
		var target = new MyClass();
		target.Id = Id;
		return target;
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
                    (typeof(Adapter<DataObjectGenerator>),"MyClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
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

        var expected = @"using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class WeatherForecastDTO
{
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public MyClassDTO MyClass { get; set; }

    /// <summary>
    /// Maps a <see cref=""WeatherForecast""/> instance to a <see cref=""WeatherForecastDTO""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""WeatherForecast""/> instance to map.</param>
    /// <returns>The mapped <see cref=""WeatherForecastDTO""/> instance.</returns>
    public WeatherForecastDTO MapFrom(WeatherForecast instance)
    {
		if (instance is null)
			return default;
		var target = new WeatherForecastDTO();
		target.Date = instance.Date;
		target.TemperatureC = instance.TemperatureC;
		return target;
	}

    /// <summary>
    /// Maps a <see cref=""WeatherForecastDTO""/> instance to a <see cref=""WeatherForecast""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""WeatherForecast""/> instance.</returns>
    public WeatherForecast MapTo()
    {
		var target = new WeatherForecast();
		target.Date = Date;
		target.TemperatureC = TemperatureC;
		return target;
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
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 10, 9, 10, 17).WithArguments("DateTime"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 12, 9, 12, 19).WithArguments("MyClassDTO"),
                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"WeatherForecastDTO.g.cs",SourceText.From(expected,Encoding.UTF8))
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

        var expectedTestingWeather = @"using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class TestingWeather
{
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public Station2 Station { get; set; }

    /// <summary>
    /// Maps a <see cref=""WeatherForecast""/> instance to a <see cref=""TestingWeather""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""WeatherForecast""/> instance to map.</param>
    /// <returns>The mapped <see cref=""TestingWeather""/> instance.</returns>
    public TestingWeather MapFrom(WeatherForecast instance)
    {
		if (instance is null)
			return default;
		var target = new TestingWeather();
		target.Date = instance.Date;
		target.TemperatureC = instance.TemperatureC;
		return target;
	}

    /// <summary>
    /// Maps a <see cref=""TestingWeather""/> instance to a <see cref=""WeatherForecast""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""WeatherForecast""/> instance.</returns>
    public WeatherForecast MapTo()
    {
		var target = new WeatherForecast();
		target.Date = Date;
		target.TemperatureC = TemperatureC;
		return target;
	}
}
";
        var expectedDto =
            """
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using MyNamespace;

namespace SourceDto;
public record class WeatherForecastDTO
{
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	[ExcludeProperty("TestingWeather")]
	public string Summary { get; set; }
	public StationDTO Station { get; set; }

    /// <summary>
    /// Maps a <see cref="WeatherForecast"/> instance to a <see cref="WeatherForecastDTO"/> instance.
    /// </summary>
    /// <param name="instance">The <see cref="WeatherForecast"/> instance to map.</param>
    /// <returns>The mapped <see cref="WeatherForecastDTO"/> instance.</returns>
    public WeatherForecastDTO MapFrom(WeatherForecast instance)
    {
		if (instance is null)
			return default;
		var target = new WeatherForecastDTO();
		target.Date = instance.Date;
		target.TemperatureC = instance.TemperatureC;
		target.Summary = instance.Summary;
		return target;
	}

    /// <summary>
    /// Maps a <see cref="WeatherForecastDTO"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		var target = new WeatherForecast();
		target.Date = Date;
		target.TemperatureC = TemperatureC;
		target.Summary = Summary;
		return target;
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
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 10, 9, 10, 17).WithArguments("DateTime"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 12, 9, 12, 17).WithArguments("Station2"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 10, 9, 10, 17).WithArguments("DateTime"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 12, 3, 12, 18).WithArguments("ExcludeProperty"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 12, 3, 12, 18).WithArguments("ExcludePropertyAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 14, 9, 14, 19).WithArguments("StationDTO"),

                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"WeatherForecastDTO.g.cs",SourceText.From(expectedDto,Encoding.UTF8)),
                    (typeof(Adapter<DataObjectGenerator>),"TestingWeather.g.cs",SourceText.From(expectedTestingWeather,Encoding.UTF8))
                }
            }
        };

        // Act & Assert
        await test.RunAsync();
    }
    [Test]
    public async Task ListWithUseExistingDTOShouldReturnExpectedCode()
    {
        var source =
            """
            using System.Collections.Generic;

            namespace Demo;
            [GenerateDto]
            public class Class
            {
                [UseExistingDto()]
                public List<WeatherForecast> WeatherForecasts { get; set; }

            }
            """;
        var expected =
            """
            using System.Collections.Generic;
            using System.Dynamic;
            using System.Linq;
            using SourceDto;
            using Demo;

            namespace SourceDto;
            public record class ClassDTO
            {
            	public List<WeatherForecastDTO> WeatherForecasts { get; set; }

                /// <summary>
                /// Maps a <see cref="Class"/> instance to a <see cref="ClassDTO"/> instance.
                /// </summary>
                /// <param name="instance">The <see cref="Class"/> instance to map.</param>
                /// <returns>The mapped <see cref="ClassDTO"/> instance.</returns>
                public ClassDTO MapFrom(Class instance)
                {
            		if (instance is null)
            			return default;
            		var target = new ClassDTO();
            		if (instance.WeatherForecasts != null)
            		{
            			target.WeatherForecasts = WeatherForecastDTO.MapFromList(instance.WeatherForecasts);
            		}
            		return target;
            	}

                /// <summary>
                /// Maps a <see cref="ClassDTO"/> instance to a <see cref="Class"/> instance.
                /// </summary>
                /// <returns>The mapped <see cref="Class"/> instance.</returns>
                public Class MapTo()
                {
            		var target = new Class();
            		if (WeatherForecasts != null)
            		{
            			target.WeatherForecasts = WeatherForecastDTO.MapToList(WeatherForecasts);
            		}
            		return target;
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
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 2, 4, 13).WithArguments("GenerateDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 2, 4, 13).WithArguments("GenerateDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 20).WithArguments("UseExistingDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 20).WithArguments("UseExistingDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(8, 17, 8, 32).WithArguments("WeatherForecast"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 10, 14, 10, 32).WithArguments("WeatherForecastDTO"),
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 24, 30, 24, 48).WithArguments("WeatherForecastDTO"),
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 38, 30, 38, 48).WithArguments("WeatherForecastDTO"),

                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"ClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8)),
                }
            }
        };

        // Act & Assert
        await test.RunAsync();
    }

    [Test]
    public async Task ArraWithUseExistingDTOShouldReturnExpectedCode()
    {
        var source =
            """
            using System.Collections.Generic;

            namespace Demo;
            [GenerateDto]
            public class Class
            {
                [UseExistingDto]
                public WeatherForecast[] WeatherForecasts { get; set; }

            }
            """;
        var expected =
            """
            using System.Collections.Generic;
            using System.Dynamic;
            using System.Linq;
            using SourceDto;
            using Demo;

            namespace SourceDto;
            public record class ClassDTO
            {
            	public WeatherForecastDTO[] WeatherForecasts { get; set; }

                /// <summary>
                /// Maps a <see cref="Class"/> instance to a <see cref="ClassDTO"/> instance.
                /// </summary>
                /// <param name="instance">The <see cref="Class"/> instance to map.</param>
                /// <returns>The mapped <see cref="ClassDTO"/> instance.</returns>
                public ClassDTO MapFrom(Class instance)
                {
            		if (instance is null)
            			return default;
            		var target = new ClassDTO();
            		target.WeatherForecasts = WeatherForecastDTO.MapFromArray(instance.WeatherForecasts);
            		return target;
            	}

                /// <summary>
                /// Maps a <see cref="ClassDTO"/> instance to a <see cref="Class"/> instance.
                /// </summary>
                /// <returns>The mapped <see cref="Class"/> instance.</returns>
                public Class MapTo()
                {
            		var target = new Class();
            		target.WeatherForecasts = WeatherForecastDTO.MapToArray(WeatherForecasts);
            		return target;
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
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 2, 4, 13).WithArguments("GenerateDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(4, 2, 4, 13).WithArguments("GenerateDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 20).WithArguments("UseExistingDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 6, 7, 20).WithArguments("UseExistingDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(8, 12, 8, 27).WithArguments("WeatherForecast"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 10, 9, 10, 27).WithArguments("WeatherForecastDTO"),
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 22, 29, 22, 47).WithArguments("WeatherForecastDTO"),
DiagnosticResult.CompilerError("CS0103").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 33, 29, 33, 47).WithArguments("WeatherForecastDTO"),

                },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"ClassDTO.g.cs",SourceText.From(expected,Encoding.UTF8)),
                }
            }
        };

        // Act & Assert
        await test.RunAsync();
    }

    [Test]
    public async Task AdvancedTestOfAScenario()
    {
        var source =
            """
using System.Collections.Generic;
using System;
using DynatestSourceGenerator.Attributes;

namespace Demo; 

[GenerateDto("WeatherForecastDTO", "TestingWeather")] 
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public int TemperatureK { get; set; }

    [ExcludeProperty("TestingWeather")] 
    public string Summary { get; set; }

    [UseExistingDto("TestingWeather > StationWithNoNameDTO")]
    public Station Station { get; set; }  
} 
""";

        var expectedTestingWeather =
            """
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using Demo;
using System;
using DynatestSourceGenerator.Attributes;

namespace SourceDto;
public record class TestingWeather
{
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
	public int TemperatureK { get; set; }
	public StationWithNoNameDTO Station { get; set; }

    /// <summary>
    /// Maps a <see cref="WeatherForecast"/> instance to a <see cref="TestingWeather"/> instance.
    /// </summary>
    /// <param name="instance">The <see cref="WeatherForecast"/> instance to map.</param>
    /// <returns>The mapped <see cref="TestingWeather"/> instance.</returns>
    public TestingWeather MapFrom(WeatherForecast instance)
    {
		if (instance is null)
			return default;
		var target = new TestingWeather();
		target.Date = instance.Date;
		target.TemperatureC = instance.TemperatureC;
		target.TemperatureK = instance.TemperatureK;
		return target;
	}

    /// <summary>
    /// Maps a <see cref="TestingWeather"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		var target = new WeatherForecast();
		target.Date = Date;
		target.TemperatureC = TemperatureC;
		target.TemperatureK = TemperatureK;
		return target;
	}
}

""";
        var expectedDto =
            """
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using SourceDto;
using Demo;
using System;
using DynatestSourceGenerator.Attributes;

namespace SourceDto;
public record class WeatherForecastDTO
{
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
	public int TemperatureK { get; set; }
	[ExcludeProperty("TestingWeather")] 
	public string Summary { get; set; }
	public StationDTO Station { get; set; }

    /// <summary>
    /// Maps a <see cref="WeatherForecast"/> instance to a <see cref="WeatherForecastDTO"/> instance.
    /// </summary>
    /// <param name="instance">The <see cref="WeatherForecast"/> instance to map.</param>
    /// <returns>The mapped <see cref="WeatherForecastDTO"/> instance.</returns>
    public WeatherForecastDTO MapFrom(WeatherForecast instance)
    {
		if (instance is null)
			return default;
		var target = new WeatherForecastDTO();
		target.Date = instance.Date;
		target.TemperatureC = instance.TemperatureC;
		target.TemperatureK = instance.TemperatureK;
		target.Summary = instance.Summary;
		return target;
	}

    /// <summary>
    /// Maps a <see cref="WeatherForecastDTO"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		var target = new WeatherForecast();
		target.Date = Date;
		target.TemperatureC = TemperatureC;
		target.TemperatureK = TemperatureK;
		target.Summary = Summary;
		return target;
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
DiagnosticResult.CompilerError("CS0246").WithSpan(3, 7, 3, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 2, 7, 13).WithArguments("GenerateDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(7, 2, 7, 13).WithArguments("GenerateDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(17, 6, 17, 21).WithArguments("ExcludeProperty"),
DiagnosticResult.CompilerError("CS0246").WithSpan(17, 6, 17, 21).WithArguments("ExcludePropertyAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(20, 6, 20, 20).WithArguments("UseExistingDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(20, 6, 20, 20).WithArguments("UseExistingDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(21, 12, 21, 19).WithArguments("Station"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 7, 7, 7, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 16, 9, 16, 29).WithArguments("StationWithNoNameDTO"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 7, 7, 7, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 16, 3, 16, 18).WithArguments("ExcludeProperty"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 16, 3, 16, 18).WithArguments("ExcludePropertyAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.3, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 18, 9, 18, 19).WithArguments("StationDTO"),
           },
                GeneratedSources =
                {
                    (typeof(Adapter<DataObjectGenerator>),"WeatherForecastDTO.g.cs",SourceText.From(expectedDto,Encoding.UTF8)),
                    (typeof(Adapter<DataObjectGenerator>),"TestingWeather.g.cs",SourceText.From(expectedTestingWeather,Encoding.UTF8))
                }
            }
        };

        // Act & Assert
        await test.RunAsync();
    }
}

