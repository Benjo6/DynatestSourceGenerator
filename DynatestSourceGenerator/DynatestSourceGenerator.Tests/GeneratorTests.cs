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

        var expected = @"using System.Dynamic;
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
		Id=instance.Id;
		Name=instance.Name;
		return this;
	}

    /// <summary>
    /// Maps a <see cref=""MyClassDTO""/> instance to a <see cref=""MyClass""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""MyClass""/> instance.</returns>
    public MyClass MapTo()
    {
		return new MyClass
		{
			Id=Id,
			Name=Name,
		};
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
		Id=instance.Id;
		return this;
	}

    /// <summary>
    /// Maps a <see cref=""MyClassDTO""/> instance to a <see cref=""MyClass""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""MyClass""/> instance.</returns>
    public MyClass MapTo()
    {
		return new MyClass
		{
			Id=Id,
		};
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

        var expected = @"using System.Dynamic;
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
		Date=instance.Date;
		TemperatureC=instance.TemperatureC;
		MyClass = new MyClassDTO().MapFrom(instance.MyClass);
		return this;
	}

    /// <summary>
    /// Maps a <see cref=""WeatherForecastDTO""/> instance to a <see cref=""WeatherForecast""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""WeatherForecast""/> instance.</returns>
    public WeatherForecast MapTo()
    {
		return new WeatherForecast
		{
			Date=Date,
			TemperatureC=TemperatureC,
			MyClass=MyClass.MapTo(),
		};
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
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 9, 9, 9, 17).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs",11, 9, 11, 19).WithArguments("MyClassDTO"),
    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 22, 17, 22, 27).WithArguments("MyClassDTO"),
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

        var expectedTestingWeather = @"using System.Dynamic;
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
		Date=instance.Date;
		TemperatureC=instance.TemperatureC;
		Station = new Station2().MapFrom(instance.Station);
		return this;
	}

    /// <summary>
    /// Maps a <see cref=""TestingWeather""/> instance to a <see cref=""WeatherForecast""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""WeatherForecast""/> instance.</returns>
    public WeatherForecast MapTo()
    {
		return new WeatherForecast
		{
			Date=Date,
			TemperatureC=TemperatureC,
			Station=Station.MapTo(),
		};
	}
}
";
        var expectedDto =
            """
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
		Date=instance.Date;
		TemperatureC=instance.TemperatureC;
		Summary=instance.Summary;
		Station = new StationDTO().MapFrom(instance.Station);
		return this;
	}

    /// <summary>
    /// Maps a <see cref="WeatherForecastDTO"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		return new WeatherForecast
		{
			Date=Date,
			TemperatureC=TemperatureC,
			Summary=Summary,
			Station=Station.MapTo(),
		};
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
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 9, 9, 9, 17).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 11, 9, 11, 17).WithArguments("Station2"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 22, 17, 22, 25).WithArguments("Station2"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 9, 9, 9, 17).WithArguments("DateTime"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 11, 3, 11, 18).WithArguments("ExcludeProperty"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 11, 3, 11, 18).WithArguments("ExcludePropertyAttribute"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 13, 9, 13, 19).WithArguments("StationDTO"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 25, 17, 25, 27).WithArguments("StationDTO"),

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
            using System.Dynamic;
            using System.Linq;
            using SourceDto;
            using Demo;
            using System.Collections.Generic;

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
            		WeatherForecasts = instance.WeatherForecasts?.Select(t=>new WeatherForecastDTO().MapFrom(t)).ToList();
            		return this;
            	}

                /// <summary>
                /// Maps a <see cref="ClassDTO"/> instance to a <see cref="Class"/> instance.
                /// </summary>
                /// <returns>The mapped <see cref="Class"/> instance.</returns>
                public Class MapTo()
                {
            		return new Class
            		{
            			WeatherForecasts = WeatherForecasts?.Select(t=>t.MapTo()).ToList(),
            		};
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
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 10, 14, 10, 32).WithArguments("WeatherForecastDTO"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 19, 63, 19, 81).WithArguments("WeatherForecastDTO"),

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
            using System.Dynamic;
            using System.Linq;
            using SourceDto;
            using Demo;
            using System.Collections.Generic;

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
            		WeatherForecasts = instance.WeatherForecasts?.Select(t=>new WeatherForecastDTO().MapFrom(t)).ToArray();
            		return this;
            	}

                /// <summary>
                /// Maps a <see cref="ClassDTO"/> instance to a <see cref="Class"/> instance.
                /// </summary>
                /// <returns>The mapped <see cref="Class"/> instance.</returns>
                public Class MapTo()
                {
            		return new Class
            		{
            			WeatherForecasts = WeatherForecasts?.Select(t=>t.MapTo()).ToArray(),
            		};
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
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 10, 9, 10, 27).WithArguments("WeatherForecastDTO"),
                    DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\ClassDTO.g.cs", 19, 63, 19, 81).WithArguments("WeatherForecastDTO"),

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
		Date=instance.Date;
		TemperatureC=instance.TemperatureC;
		TemperatureK=instance.TemperatureK;
		Station = new StationWithNoNameDTO().MapFrom(instance.Station);
		return this;
	}

    /// <summary>
    /// Maps a <see cref="TestingWeather"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		return new WeatherForecast
		{
			Date=Date,
			TemperatureC=TemperatureC,
			TemperatureK=TemperatureK,
			Station=Station.MapTo(),
		};
	}
}

""";
        var expectedDto =
            """
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
		Date=instance.Date;
		TemperatureC=instance.TemperatureC;
		TemperatureK=instance.TemperatureK;
		Summary=instance.Summary;
		Station = new StationDTO().MapFrom(instance.Station);
		return this;
	}

    /// <summary>
    /// Maps a <see cref="WeatherForecastDTO"/> instance to a <see cref="WeatherForecast"/> instance.
    /// </summary>
    /// <returns>The mapped <see cref="WeatherForecast"/> instance.</returns>
    public WeatherForecast MapTo()
    {
		return new WeatherForecast
		{
			Date=Date,
			TemperatureC=TemperatureC,
			TemperatureK=TemperatureK,
			Summary=Summary,
			Station=Station.MapTo(),
		};
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
DiagnosticResult.CompilerError("CS0246").WithSpan(2, 7, 2, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan(6, 2, 6, 13).WithArguments("GenerateDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(6, 2, 6, 13).WithArguments("GenerateDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(16, 6, 16, 21).WithArguments("ExcludeProperty"),
DiagnosticResult.CompilerError("CS0246").WithSpan(16, 6, 16, 21).WithArguments("ExcludePropertyAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(19, 6, 19, 20).WithArguments("UseExistingDto"),
DiagnosticResult.CompilerError("CS0246").WithSpan(19, 6, 19, 20).WithArguments("UseExistingDtoAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan(20, 12, 20, 19).WithArguments("Station"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 6, 7, 6, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 15, 9, 15, 29).WithArguments("StationWithNoNameDTO"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\TestingWeather.g.cs", 27, 17, 27, 37).WithArguments("StationWithNoNameDTO"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 6, 7, 6, 30).WithArguments("DynatestSourceGenerator"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 15, 3, 15, 18).WithArguments("ExcludeProperty"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 15, 3, 15, 18).WithArguments("ExcludePropertyAttribute"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs", 17, 9, 17, 19).WithArguments("StationDTO"),
DiagnosticResult.CompilerError("CS0246").WithSpan("DynatestSourceGenerator.Tests\\DynatestSourceGenerator.Tests.Helpers.Adapter`1[[DynatestSourceGenerator.DataTransferObject.DataObjectGenerator, DynatestSourceGenerator, Version=0.0.4.2, Culture=neutral, PublicKeyToken=null]]\\WeatherForecastDTO.g.cs",30, 17, 30, 27).WithArguments("StationDTO"),
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

