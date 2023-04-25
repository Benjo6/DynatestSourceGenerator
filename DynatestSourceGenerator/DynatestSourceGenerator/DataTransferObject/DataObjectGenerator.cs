#nullable enable
using System;
using System.Collections.Generic;
using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using DynatestSourceGenerator.DataTransferObject.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DynatestSourceGenerator.DataTransferObject;

[Generator(LanguageNames.CSharp)]
public class DataObjectGenerator : IIncrementalGenerator
{

    private const string GeneratedFileSuffix = ".g.cs";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
#if DEBUG
if (!Debugger.IsAttached) 
{ 
Debugger.Launch(); 
}
#endif

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

        var stringBuilders = new List<StringBuilder>();
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

            var arguments = Get.AttributeArguments(hasGenerateAttribute)
                .DefaultIfEmpty($"{classDeclarationSyntax.Identifier.ValueText}DTO");

            BuildClass(stringBuilders, arguments, classDeclarationSyntax);
        }

        foreach (var classBuilder in stringBuilders)
        {
            var classStartIndex =
                classBuilder.ToString().IndexOf("public record class ") + "public record class ".Length;
            var classEndIndex = classBuilder.ToString().IndexOf("\n{", classStartIndex);
            var className = classBuilder.ToString().Substring(classStartIndex, classEndIndex - classStartIndex).Trim();
            
            BuildMethods(stringBuilders, className);
            classBuilder.AppendLine("\n}");


            context.AddSource($"{className}{GeneratedFileSuffix}", classBuilder.ToString());
        }
    }

    private static void BuildMethods(List<StringBuilder> stringBuilders, string dataTransferObject)
    {
        foreach (var classBuilder in stringBuilders)
        {
            
            var paramStartIndex = classBuilder.ToString().IndexOf("<see cref=\"") + "<see cref=\"".Length;
            var paramEndIndex = classBuilder.ToString().IndexOf("\"", paramStartIndex);
            string param = classBuilder.ToString().Substring(paramStartIndex, paramEndIndex - paramStartIndex).Trim();
            var classStartIndex =
                classBuilder.ToString().IndexOf("public record class ") + "public record class ".Length;
            var classEndIndex = classBuilder.ToString().IndexOf("\n{", classStartIndex);
            var className = classBuilder.ToString().Substring(classStartIndex, classEndIndex - classStartIndex).Trim();
            if (className == dataTransferObject)
            {
                foreach (var stringBuilder in stringBuilders)
                {
                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromArray") >= 0)
                    {
                        // MapFromArray
                        classBuilder.AppendLine($@"
    /// <summary>
    /// Maps an array of objects of type <see cref=""{param}""/> to an array of objects of type <see cref=""{className}""/>
    /// </summary>
    /// <param name=""source"">The array of objects of type <see cref=""{param}""/> to be mapped.</param>
    /// <returns>The mapped array of objects of type <see cref=""{className}""/>.</returns>");
                        Map.FromArrayMethod(classBuilder, className, param);
                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapToArray") >= 0)
                    {
                        // MapToArray
                        classBuilder.AppendLine($@"
    /// <summary>
    /// Maps an array of objects of type <see cref=""{className}""/> to an array of objects of type <see cref=""{param}""/>.
    /// </summary>
    /// <param name=""source"">The array of objects of type <see cref=""{className}""/> to be mapped.</param>
    /// <returns>The mapped array of objects of type <see cref=""{param}""/>.</returns>");
                        Map.ToArrayMethod(classBuilder, param, className);
                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromList") >= 0)
                    {
                        // MapFromList
                        classBuilder.AppendLine($@"
    /// <summary>
    /// Maps an <see cref=""IEnumerable""/> of objects of type <see cref=""{param}""/> to a <see cref=""List""/> of objects of type <see cref=""{className}""/>.
    /// </summary>
    /// <param name=""source"">The <see cref=""IEnumerable""/> of objects of type <see cref=""{param}""/> to be mapped.</param>
    /// <returns>The mapped <see cref=""List""/> of objects of type <see cref=""{className}""/>.</returns>");
                        Map.FromEnumerableMethod(classBuilder, className, param);
                    }
                    
                    if (stringBuilder.ToString().IndexOf($"{className}.MapToList") >= 0)
                    {
                        // MapToList
                        classBuilder.AppendLine($@"
    /// <summary>
    /// Maps an <see cref=""IEnumerable""/> of objects of type <see cref=""{className}""/> to a <see cref=""List""/> of objects of type <see cref=""{param}""/>.
    /// </summary>
    /// <param name=""source"">The <see cref=""IEnumerable""/> of objects of type <see cref=""{className}""/> to be mapped.</param>
    /// <returns>The mapped <see cref=""List""/> of objects of type <see cref=""{param}""/>.</returns>");
                        Map.ToEnumerableMethod(classBuilder, param, className);
                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromDictionary") >= 0)
                    {
                        Map.FromDictionaryMethod(classBuilder, className, param);

                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapToDictionary") >= 0)
                    {
                        Map.ToDictionaryMethod(classBuilder, param, className);
                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromIDictionary") >= 0)
                    {
                        Map.FromIDictionaryMethod(classBuilder, className, param);

                    }
                    if (stringBuilder.ToString().IndexOf($"{className}.MapToIDictionary") >= 0)
                    {
                        Map.ToIDictionaryMethod(classBuilder, param, className);
                    }
                    
                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromIReadOnlyDictionary") >= 0)
                    {
                        Map.FromIReadOnlyDictionaryMethod(classBuilder, className, param);

                    }
                    if (stringBuilder.ToString().IndexOf($"{className}.MapToIReadOnlyDictionary") >= 0)
                    {
                        Map.ToIReadOnlyDictionaryMethod(classBuilder, param, className);
                    }

                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromStack") >= 0)
                    {
                        Map.FromStack(classBuilder, className, param);

                    }
                    if (stringBuilder.ToString().IndexOf($"{className}.MapToStack") >= 0)
                    {
                        Map.ToStack(classBuilder, param, className);
                    }
                    
                    if (stringBuilder.ToString().IndexOf($"{className}.MapFromQueue") >= 0)
                    {
                        Map.FromQueue(classBuilder, className, param);

                    }
                    if (stringBuilder.ToString().IndexOf($"{className}.MapToQueue") >= 0)
                    {
                        Map.ToQueue(classBuilder, param, className);
                    }
                }

                //Map.FromKeyValuePairMethod(classWithoutExcludedProperties, className, originalClass, classBuilder);

                //Map.ToKeyValuePairMethod(classWithoutExcludedProperties, originalClass, className, classBuilder);
            }

        }
    }

    private static void BuildClass(ICollection<StringBuilder> stringBuilders, IEnumerable<string> arguments,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        foreach (var className in arguments)
        {
            var classWithoutExcludedProperties =
                Remove.ExcludedProperties(classDeclarationSyntax, className);
            var classBuilder = new StringBuilder();
            classBuilder.AppendLine("using System.Collections.Generic;");
            classBuilder.AppendLine("using System.Dynamic;");
            classBuilder.AppendLine("using System.Linq;");
            classBuilder.AppendLine("using SourceDto;");

            Append.NamespacesToFile(classDeclarationSyntax, classBuilder);

            foreach (var usingDirective in Directives.Using(classDeclarationSyntax))
            {
                string us = usingDirective.ToString();

                // Check if the namespace has already been added to the class builder
                if (!classBuilder.ToString().Contains(us))
                {
                    // If the namespace hasn't been added, append it to the class builder
                    classBuilder.AppendLine(us);
                }
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
                classBuilder.AppendLine($"\t\tif (instance is null)\r\n\t\t\treturn default;");
                classBuilder.AppendLine($"\t\tvar target = new {className}();");
                foreach (var property in Map.FromProperties(classWithoutExcludedProperties, className))
                {
                    classBuilder.AppendLine($"\t\t{property}");
                }

                classBuilder.AppendLine("\t\treturn target;");

                classBuilder.AppendLine("\t}");

                // MapTo
                classBuilder.AppendLine($@"
    /// <summary>
    /// Maps a <see cref=""{className}""/> instance to a <see cref=""{param}""/> instance.
    /// </summary>
    /// <returns>The mapped <see cref=""{param}""/> instance.</returns>
    public {param} MapTo()
    {{");
                classBuilder.AppendLine($"\t\tvar target = new {param}();");

                foreach (var property in Map.ToProperties(classWithoutExcludedProperties, className))
                {
                    classBuilder.AppendLine($"\t\t{property}");
                }

                classBuilder.AppendLine("\t\treturn target;");
                classBuilder.AppendLine("\t}");
                stringBuilders.Add(classBuilder);

            }

        }

    }
}