using Microsoft.CodeAnalysis;

namespace SourceGenerator.Tests.Helpers;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
where  TSourceGenerator : ISourceGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator,XUnitVerifer>
    {
        
    }
}