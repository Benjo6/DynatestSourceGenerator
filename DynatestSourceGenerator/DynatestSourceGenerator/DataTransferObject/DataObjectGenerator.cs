#nullable enable
using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
        var attributeSyntax = (AttributeSyntax)context.Node;
        return attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax ? null : classDeclarationSyntax;
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not AttributeSyntax attributeSyntax)
        {
            return false;
        }

        var name = ExtractAttributeName(attributeSyntax.Name);

        return name is "GenerateDto";
    }

    private static string ExtractAttributeName(NameSyntax? name) =>
        (name switch
        {
            SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.Text,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier.Text,
            _ => null
        })!;

    private static void GenerateClass(SourceProductionContext context,
        ImmutableArray<ClassDeclarationSyntax?> enumerations)
    {
        foreach (var classDeclarationSyntax in enumerations)
        {
            var attributes = from attributeList in classDeclarationSyntax!.AttributeLists
                             from attribute in attributeList.Attributes
                             select attribute;

            var hasGenerateAttribute = attributes.FirstOrDefault(a => a.Name.ToString() == nameof(GenerateDto));
            if (hasGenerateAttribute is null)
            {
                continue;
            }


            var arguments = Methods.GetAttributeArguments(hasGenerateAttribute);
            if (!arguments.Any())
            {
                arguments.Add($"{classDeclarationSyntax.Identifier.ValueText}DTO");
            }

            foreach (var className in arguments)
            {
                var classWithoutExcludedProperties =
                    Methods.RemoveExcludedProperties(classDeclarationSyntax, className);
                var classBuilder = new StringBuilder();

                classBuilder.AppendLine("using System.Dynamic;");
                classBuilder.AppendLine("using System.Collections;");
                classBuilder.AppendLine("using SourceDto;");
                Methods.AppendNamespacesToFile(classDeclarationSyntax, classBuilder);

                foreach (var usingDirective in Methods.UsingDirectives(classDeclarationSyntax))
                {
                    classBuilder.AppendLine(usingDirective!.ToString());
                }


                classBuilder.AppendLine($@"
namespace SourceDto;
public record class {className}
{{");
                if (Methods.GetProperties(classWithoutExcludedProperties, className).Any())
                {
                    foreach (var property in Methods.GetProperties(classWithoutExcludedProperties, className))
                    {
                        classBuilder.AppendLine($"\t{property}");
                    }

                    // MapFrom
                    var param = classWithoutExcludedProperties!.Identifier.ValueText;
                    classBuilder.AppendLine($@"
    /// <summary>
    /// Maps a <see cref=""{param}""/> instance to a <see cref=""{className}""/> instance.
    /// </summary>
    /// <param name=""instance"">The <see cref=""{param}""/> instance to map.</param>
    /// <returns>The mapped <see cref=""{className}""/> instance.</returns>
    public {className} MapFrom({param} instance)
    {{");
                    foreach (var property in Methods.GetMapFromProperties(classWithoutExcludedProperties, className))
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

                    foreach (var property in Methods.GetMapToProperties(classWithoutExcludedProperties, className))
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