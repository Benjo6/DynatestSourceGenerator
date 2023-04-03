using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PropertyDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax;

namespace DynatestSourceGenerator;

internal static class Methods
{
    #region Maps

    public static List<string> GetMapFromProperties(SyntaxNode? classDeclarationSyntax, string className)
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
                if (name.Contains("<") && name.Contains("DTO"))
                {
                    // Find the index of the first '<' character in the string
                    int startIndex = name.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    int endIndex = name.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    string type = name.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Replace the type parameter with the modified type
                    name = $"{type}DTO";
                    
                    props.Add($"{propertyDeclaration.Identifier} = instance.{propertyDeclaration.Identifier}?.Select(t=>new {name}().MapFrom(t)).ToList();");
                    continue;
                }
                
                props.Add(
                    $"{propertyDeclaration.Identifier} = new {name}().MapFrom(instance.{propertyDeclaration.Identifier});");
            }
        }

        return props;
    }

    public static List<string> GetMapToProperties(SyntaxNode? classDeclarationSyntax, string className)
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
                    props.Add($"{propertyName} = {propertyName},");
                }
            }
            else
            {
                if (propertyDeclaration.ToString().Contains("<"))
                {
                    props.Add(
                        $"{propertyDeclaration.Identifier} = {propertyDeclaration.Identifier}?.Select(t=>t.MapTo()).ToList(),");
                    continue;
                }


                props.Add(
                    $"{propertyDeclaration.Identifier} = {propertyDeclaration.Identifier}.MapTo(),");
            }
        }

        return props;
    }

    #endregion

    public static void AppendNamespacesToFile(ClassDeclarationSyntax classDeclarationSyntax, StringBuilder classBuilder)
    {
        if (NamespaceDirectives(classDeclarationSyntax) != null && NamespaceDirectives(classDeclarationSyntax).Any())
        {
            foreach (var namespaceDirective in NamespaceDirectives(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name};");
            }
        }
        else
        {
            foreach (var namespaceDirective in FileScopedNamespaceDirectives(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name};");
            }
        }
    }


    public static IEnumerable<UsingDirectiveSyntax?> UsingDirectives(SyntaxNode classDeclarationSyntax)
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
        SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as FileScopedNamespaceDeclarationSyntax).WhereNotNull();
    }


    public static IEnumerable<string> GetProperties(SyntaxNode? classDeclarationSyntax, string className)
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
                else if (propertyDeclaration.AccessorList == null ||
                         propertyDeclaration.AccessorList.Accessors.All(a =>
                             a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                }
                else if (propertyDeclaration.ToString().StartsWith("["))
                {
                    var prop1 = propertyDeclaration.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
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
                var dto = propertyDeclaration.ToString().GetLastPart("]").ReplaceFirst(propertyDeclaration.Type.ToString(),
                    replace ?? $"{propertyDeclaration.Type}DTO");
                
                if (dto.Contains("<") && dto.Contains("DTO"))
                {
                    // Find the index of the first '<' character in the string
                    int startIndex = dto.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    int endIndex = dto.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    string type = dto.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Replace the type parameter with the modified type
                    dto = dto.Replace($"<{type}>DTO", $"<{type}DTO>");
                }
                props.Add($"{dto.TrimStart()}");
            }
        }

        return props;
    }


    private static string? GetUsingArgument(AttributeSyntax usingSyntax, string className)
    {
        var argument = GetAttributeArguments(usingSyntax)
            .Where(u => u.StartsWith(className) && u.Contains(" > "));
        return argument.FirstOrDefault()?.Split('>').LastOrDefault()?.Trim();
    }

    public static List<string> GetAttributeArguments(AttributeSyntax attribute)
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


    public static ClassDeclarationSyntax? RemoveExcludedProperties(ClassDeclarationSyntax classDeclaration,
        string className)
    {
        var ignoredProperties = classDeclaration.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString() == nameof(ExcludeProperty) &&
                          (!GetAttributeArguments(a).Any() || GetAttributeArguments(a).Contains(className))));

        var getClassWithoutIgnoredProperties =
            classDeclaration.RemoveNodes(ignoredProperties, SyntaxRemoveOptions.KeepEndOfLine);

        return getClassWithoutIgnoredProperties;
    }
}