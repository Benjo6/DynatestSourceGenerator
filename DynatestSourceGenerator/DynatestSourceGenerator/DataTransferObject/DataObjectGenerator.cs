#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using DynatestSourceGenerator.DataTransferObject.Attributes;
using DynatestSourceGenerator.DataTransferObject.Enums;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax;

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


            var arguments = GetAttributeArguments(hasGenerateAttribute);
            if (!arguments.Any())
            {
                arguments.Add($"{classDeclarationSyntax.Identifier.ValueText}DTO");
            }
            
            foreach (var className in arguments)
            {
                var classWithoutExcludedProperties = RemoveExcludedProperties(classDeclarationSyntax,className);
                var classBuilder = new StringBuilder();
                
                classBuilder.AppendLine("using System.Dynamic;");
                classBuilder.AppendLine("using System.Collections;");
                classBuilder.AppendLine("using SourceDto;");
                AppendNamespacesToFile(classDeclarationSyntax, classBuilder);

                foreach (var usingDirective in UsingDirectives(classDeclarationSyntax))
                {
                    classBuilder.AppendLine(usingDirective!.ToString());
                }


                classBuilder.AppendLine($@"
namespace SourceDto;
public record class {className}
{{");
                foreach (var property in GetProperties(classWithoutExcludedProperties, className))
                {
                    classBuilder.AppendLine($"\t{property}");
                }

                var param = classWithoutExcludedProperties!.Identifier.ValueText;
                classBuilder.AppendLine($@"
    public {className} Map({param} instance)
    {{");
                foreach (var property in GetMappingProperties(classWithoutExcludedProperties,className))
                {
                    classBuilder.AppendLine($"\t\t{property}");
                }

                classBuilder.AppendLine("\t\treturn this;");

                classBuilder.AppendLine("\t}");
                classBuilder.AppendLine("}");


                context.AddSource($"{className}{GeneratedFileSuffix}",
                    classBuilder.ToString());
            }
        }
    }

    private static void AppendNamespacesToFile(ClassDeclarationSyntax classDeclarationSyntax, StringBuilder classBuilder)
    {
        if (NamespaceDirectives(classDeclarationSyntax) != null && NamespaceDirectives(classDeclarationSyntax).Any())
        {
            foreach (var namespaceDirective in NamespaceDirectives(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name.ToString()};");
            }
        }
        else
        {
            foreach (var namespaceDirective in FileScopedNamespaceDirectives(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name.ToString()};");
            }
        }
    }


    private static IEnumerable<UsingDirectiveSyntax?> UsingDirectives(SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as UsingDirectiveSyntax).WhereNotNull();
    }

    private static IEnumerable<NamespaceDeclarationSyntax?> NamespaceDirectives(SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as NamespaceDeclarationSyntax).WhereNotNull();
    }

    private static IEnumerable<FileScopedNamespaceDeclarationSyntax?> FileScopedNamespaceDirectives(
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as FileScopedNamespaceDeclarationSyntax).WhereNotNull();
    }

    private static List<string> GetMappingProperties(SyntaxNode? classDeclarationSyntax, string className)
    {
        var props = new List<string>();
        foreach (var property in classDeclarationSyntax!.ChildNodes())
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;
            
            var propertyName = propertyDeclaration.Identifier.Text;
            var useExisting = GetUsingExistingAttribute(propertyDeclaration);
            if (useExisting == null)
            {
                if (propertyDeclaration.AccessorList == null ||
                    propertyDeclaration.AccessorList.Accessors.All(a => a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                }
                else
                {
                    props.Add($"{propertyName} = instance.{propertyName};");
                }
            }
            else
            {
                var replace = GetUsingArgument(useExisting, className);
                var name = replace ?? $"{propertyDeclaration.Type}DTO";
                props.Add(
                    $"{propertyDeclaration.Identifier} = new {name}().Map(instance.{propertyDeclaration.Identifier});");
            }
        }

        return props;
    }

    private static IEnumerable<string> GetProperties(SyntaxNode? classDeclarationSyntax, string className)
    {
        var props = new List<string>();
        foreach (var property in classDeclarationSyntax!.ChildNodes())
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;
            

            var useExisting = GetUsingExistingAttribute(propertyDeclaration);
            if (useExisting == null)
            {
                if (propertyDeclaration.ExpressionBody is not null)
                {
                    props.Add(
                        $"{propertyDeclaration.Modifiers} {propertyDeclaration.Type} {propertyDeclaration.Identifier.Text} {propertyDeclaration.ExpressionBody};");
                }
                else if(propertyDeclaration.AccessorList == null ||
                        propertyDeclaration.AccessorList.Accessors.All(a =>
                            a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                }
                else if (propertyDeclaration.ToString().StartsWith("["))
                {
                    var prop1 = propertyDeclaration.ToString().Split("\r\n");
                    props.Add($"{prop1.First()}\r\n\t{prop1.Last().TrimStart()}");
                }
                else
                {
                    props.Add($"{propertyDeclaration}");
                }
            }
            else
            {
                var replace = GetUsingArgument(useExisting, className);
                var dto = propertyDeclaration.ToString().GetLastPart("]")
                    .ReplaceFirst(propertyDeclaration.Type.ToString(), replace ?? $"{propertyDeclaration.Type}DTO").TrimStart();

                props.Add($"{dto}");
            }
        }

        return props;
    }
    

    private static string? GetUsingArgument(AttributeSyntax usingSyntax, string className)
    {
        var argument = GetAttributeArguments(usingSyntax)
            .Where(u => u.StartsWith(className) && u.Contains(" > "));
        return argument.FirstOrDefault()?.Split(" > ")[1];
    }

    private static List<string> GetAttributeArguments(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList is null)
        {
            return new List<string>();
        }

        var arguments = attribute.ArgumentList.Arguments
            .Select(s => s.NormalizeWhitespace().ToFullString().Replace("\"", "")).ToList();

        return arguments;
    }

    private static AttributeSyntax? GetUsingExistingAttribute(MemberDeclarationSyntax property)
    {
        return property.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString() == nameof(UseExistingDto));
    }
    
    
    private static ClassDeclarationSyntax? RemoveExcludedProperties(ClassDeclarationSyntax classDeclaration, string className)
    {
        var ignoredProperties  = classDeclaration.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString() == nameof(ExcludeProperty) && 
                          (!GetAttributeArguments(a).Any() || GetAttributeArguments(a).Contains(className))));
            
        var getClassWithoutIgnoredProperties =
            classDeclaration.RemoveNodes(ignoredProperties, SyntaxRemoveOptions.KeepEndOfLine);
        
        return getClassWithoutIgnoredProperties;
    }
    

    
   
}