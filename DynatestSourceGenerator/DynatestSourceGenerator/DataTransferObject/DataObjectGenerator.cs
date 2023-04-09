#nullable enable
using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DynatestSourceGenerator.DataTransferObject.Utilities;

namespace DynatestSourceGenerator.DataTransferObject;

[Generator(LanguageNames.CSharp)]
public class DataObjectGenerator : IIncrementalGenerator
{

    private const string GeneratedFileSuffix = ".g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {

        var classDeclarationSyntax =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: (node, _) => IsSyntaxTargetForGeneration(node),
                    transform: (syntaxContext, _) => GetSemanticTargetForGeneration(syntaxContext))
                .WhereNotNull().Collect();

        context.RegisterSourceOutput(classDeclarationSyntax, GenerateClass);
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        if (context.Node is not AttributeSyntax attributeSyntax)
        {
            return null;
        }

        var name = Get.AttributeName(attributeSyntax.Name);

        if (name is not "GenerateDto")
        {
            return null;
        }

        return attributeSyntax.Parent?.Parent as ClassDeclarationSyntax;
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
        {
            return false;
        }

        var name = Get.AttributeName(attributeSyntax.Name);

        return name is "GenerateDto";
    }

    

    private static void GenerateClass(SourceProductionContext context,
        ImmutableArray<ClassDeclarationSyntax?> enumerations)
    {
        foreach (var classDeclarationSyntax in enumerations)
        {
            if (classDeclarationSyntax is null) continue;

            var attributes = from attributeList in classDeclarationSyntax.AttributeLists
                             from attribute in attributeList.Attributes
                             select attribute;

            var hasGenerateAttribute = attributes.FirstOrDefault(a => a.Name.ToString() == nameof(GenerateDto));
            if (hasGenerateAttribute is null)
            {
                continue;
            }

            var arguments = Get.AttributeArguments(hasGenerateAttribute).DefaultIfEmpty($"{classDeclarationSyntax.Identifier.ValueText}DTO");

            foreach (var className in arguments)
            {
                var classWithoutExcludedProperties =
                    Remove.ExcludedProperties(classDeclarationSyntax, className);
                var classBuilder = new StringBuilder();
                classBuilder.AppendLine("using System.Dynamic;");
                classBuilder.AppendLine("using System.Linq;");
                classBuilder.AppendLine("using SourceDto;");

                Append.NamespacesToFile(classDeclarationSyntax, classBuilder);

                foreach (var usingDirective in Directives.Using(classDeclarationSyntax))
                {
                    classBuilder.AppendLine(usingDirective!.ToString());
                }
                classBuilder.AppendLine($@"
namespace SourceDto;
public record class {className}
{{");
                var properties = Get.Properties(classWithoutExcludedProperties, className).ToList();
                if (properties.Any())
                {
                    foreach (var property in properties)
                    {
                        classBuilder.AppendLine($"\t{property}");
                    }
                    var param = classWithoutExcludedProperties!.Identifier.ValueText;

                    // MapFrom
                    classBuilder.AppendLine($@"
    /// <summary>
    /// Maps a <see cref=""{param}""/> instance to a <see cref=""{className}""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""{param}""/> instance to map.</param>
    /// <returns>The mapped <see cref=""{className}""/> instance.</returns>
    public {className} MapFrom({param} instance)
    {{");
                    foreach (var property in Map.FromProperties(classWithoutExcludedProperties, className))
                    {
                        classBuilder.AppendLine($"\t\t{property}");
                    }

                    classBuilder.AppendLine("\t\treturn this;");

                    classBuilder.AppendLine("\t}");

                    // MapTo
                    classBuilder.AppendLine($@"
    /// <summary>
    /// Maps a <see cref=""{className}""/> instance to a <see cref=""{param}""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""{param}""/> instance.</returns>
    public {param} MapTo()
    {{");
                    classBuilder.AppendLine($"\t\treturn new {param}");
                    classBuilder.AppendLine("\t\t{");

                    foreach (var property in Map.ToProperties(classWithoutExcludedProperties, className))
                    {
                        classBuilder.AppendLine($"\t\t\t{property}");
                    }

                    classBuilder.AppendLine("\t\t};");

                    classBuilder.AppendLine("\t}");
                }

                classBuilder.AppendLine("}");


                context.AddSource($"{className}{GeneratedFileSuffix}",
                    classBuilder.ToString());
            }
        }
    }
}