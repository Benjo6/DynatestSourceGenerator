using System.Collections.Generic;
using System.Collections.Immutable;
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
public class Generator : IIncrementalGenerator
{
    private const string GeneratedFileSuffix = ".g.cs";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classDeclarationSyntax =
            context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: (node, _) => IsSyntaxTargetForGeneration(node),
                    transform: (syntaxContext, _) => GetSemanticTargetForGeneration(syntaxContext))
                .WhereNotNull().Collect();
        
        context.RegisterSourceOutput(classDeclarationSyntax,GenerateClass);

    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var attributeSyntax = (AttributeSyntax)context.Node;
        if (attributeSyntax.Parent?.Parent is not ClassDeclarationSyntax classDeclarationSyntax) return null;

        return classDeclarationSyntax;
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
        name switch
        {
            SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.Text,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier.Text,
            _ => null
        };
    
    
    private static IEnumerable<string> GetProperties(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var props = new List<string>();
        foreach (var child in classDeclarationSyntax.ChildNodes())
        {
            //todo 
            if (child is PropertyDeclarationSyntax prop)
            {
                if (!prop.ToString().Contains("ExcludeFromDto") && !prop.ToString().StartsWith("private"))
                {
                    props.Add(prop.ToString());
                }
            }
        }

        return props;
    }

    private static string GenerateClass(SourceProductionContext context,
        ImmutableArray<ClassDeclarationSyntax> enumerations)
    {
        var classBuilder = new StringBuilder();
            
        classBuilder.AppendLine("using System.Dynamic;");
        classBuilder.AppendLine("using System.Collections;");
        classBuilder.AppendLine("using SourceDto;");
        foreach (var namespaceDirective)
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
            
        foreach (var property in enumerations.FirstOrDefault().ChildNodes())
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