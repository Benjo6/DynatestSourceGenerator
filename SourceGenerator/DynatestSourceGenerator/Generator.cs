using System.Collections.Generic;
using System.Linq;
using System.Text;
using DynatestSourceGenerator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using DynatestSourceGenerator.Attributes;
using Microsoft.CodeAnalysis.CSharp;

namespace DynatestSourceGenerator;

[Generator]
public class Generator : ISourceGenerator
{
    private const string GeneratedFileSuffix = ".g.cs";
    
    public void Initialize(GeneratorInitializationContext context)
    {
/*#if DEBUG
            if (!Debugger.IsAttached) 
            { 
                Debugger.Launch(); 
            }
 #endif*/
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var nodes = from tree in context.Compilation.SyntaxTrees
            from node in tree.GetRoot().DescendantNodes()
            select node;

         var classDeclarationSyntaxes = nodes
            .Select(s => s as ClassDeclarationSyntax)
            .WhereNotNull();



        foreach (var classDeclarationSyntax in classDeclarationSyntaxes)
        {
            var attributes = from attributeList in classDeclarationSyntax.AttributeLists
                from attribute in attributeList.Attributes
                select attribute;

            var hasGenerateAttribute = attributes.FirstOrDefault(a => a.Name.ToString() == nameof(GenerateDto));
            if (hasGenerateAttribute is null)
            {
                continue;
            }

            var originalName = classDeclarationSyntax.Identifier.ValueText;
            var arguments = GetAttributeArguments(hasGenerateAttribute);
            var useDynamic = ExtractBool(arguments);
            if (!arguments.Any())
            {
                arguments.Add($"{originalName}DTO");
            }

            foreach (var className in arguments)
            {
                var ignoredProperties = GetIgnoredProperties(classDeclarationSyntax, className);
                var getClassWithoutIgnoredProperties =
                    classDeclarationSyntax.RemoveNodes(ignoredProperties, SyntaxRemoveOptions.KeepEndOfLine);
                if (getClassWithoutIgnoredProperties is null)
                {
                    continue;
                }
                
                var properties = getClassWithoutIgnoredProperties.ChildNodes()
                    .Select(s => s as PropertyDeclarationSyntax)
                    .WhereNotNull();
                
                var directives = classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
                    .Select(s => s as UsingDirectiveSyntax)
                    .WhereNotNull();
                
                var namespaces = classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
                    .Select(s => s as NamespaceDeclarationSyntax).WhereNotNull();

                var generatedClass = GenerateClass(originalName, className, namespaces, directives, properties, useDynamic);
                context.AddSource(className+GeneratedFileSuffix, SourceText.From(generatedClass, Encoding.UTF8));
            }
        }
    }

    private static IEnumerable<SyntaxNode> GetIgnoredProperties(ClassDeclarationSyntax declaration, string className)
    {
        var ignoredProperties  = declaration.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString() == nameof(ExcludeProperty) && 
                          (!GetAttributeArguments(a).Any() || GetAttributeArguments(a).Contains(className))));
            
        return ignoredProperties ;
    }

    private static AttributeSyntax GetUsingExistingAttribute(PropertyDeclarationSyntax property)
    {
        return property.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString() == nameof(UseExistingDto));
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

    private static bool ExtractBool(IList<string> arguments)
    {
        if (!arguments.Any() || !bool.TryParse(arguments.First(), out var parsedValue)) return false;
        arguments.RemoveAt(0);
        return parsedValue;
    }

    private static string GetUsingArgument(AttributeSyntax usingSyntax, string className)
    {
        var argument = GetAttributeArguments(usingSyntax)
            .Where(u => u.StartsWith(className) && u.Contains(" > "));
        return argument.FirstOrDefault()?.Split(" > ")[1];
    }
        
    private static string GenerateClass(string originalName, string className, IEnumerable<NamespaceDeclarationSyntax> namespaces, IEnumerable<UsingDirectiveSyntax> usingDirectives, 
        IEnumerable<PropertyDeclarationSyntax> properties, bool useDynamic)
    {
        var classBuilder = new StringBuilder();
            
        classBuilder.AppendLine("using System.Dynamic;");
        classBuilder.AppendLine("using System.Collections;");
        classBuilder.AppendLine("using SourceDto;");
        foreach (var namespaceDirective in namespaces)
        {
            classBuilder.AppendLine($"using {namespaceDirective.Name.ToString()};");
        }

        foreach (var usingDirective in usingDirectives)
        {
            classBuilder.AppendLine(usingDirective.ToString());
        }
            
        classBuilder.AppendLine($@"
namespace SourceDto
{{
    public class {className}
    {{");
            
        foreach (var property in properties)
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;
            var useExisting = GetUsingExistingAttribute(property);
            if (useExisting == null)
            {
                if (propertyDeclaration.AccessorList == null ||propertyDeclaration.AccessorList.Accessors.All(a => a.Kind() != SyntaxKind.SetAccessorDeclaration) && propertyDeclaration.ExpressionBody is not null)
                {
                    classBuilder.AppendLine($"\t\t{propertyDeclaration.Modifiers} {propertyDeclaration.Type} {propertyDeclaration.Identifier.Text} {property.ExpressionBody};");
                }
                else 
                {
                    classBuilder.AppendLine($"\t\t{propertyDeclaration}");
                }
            }
            else
            {
                var replace = GetUsingArgument(useExisting, className);
                var dto = propertyDeclaration.ToString().GetLastPart("]")
                    .ReplaceFirst(propertyDeclaration.Type.ToString(), replace ?? $"{propertyDeclaration.Type}DTO");

                classBuilder.AppendLine($"\t\t{dto}");
            }
        }
        

        var param = useDynamic ? "dynamic" : originalName;
        classBuilder.AppendLine($@"
        public {className} Map({param} instance)
        {{");
        foreach (var property in properties)
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;
            var propertyName = propertyDeclaration.Identifier.Text;
            var useExisting = GetUsingExistingAttribute(property);
            if (useExisting == null)
            {
                if (propertyDeclaration.AccessorList == null || propertyDeclaration.AccessorList.Accessors.All(a => a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                    
                }
                else
                {
                    classBuilder.AppendLine($"\t\t\t{propertyName} = instance.{propertyName};");
                }
            }
            else
            {
                var replace = GetUsingArgument(useExisting, className);
                var name = replace ?? $"{property.Type}DTO";
                classBuilder.AppendLine(
                    $"\t\t\t{property.Identifier} = new {name}().Map(instance.{property.Identifier});");
            }
        }
            
        classBuilder.AppendLine("\t\t\treturn this;");
            
        classBuilder.AppendLine("\t\t}");
        classBuilder.AppendLine("    }");
        return classBuilder.AppendLine("}").ToString();
    }
}