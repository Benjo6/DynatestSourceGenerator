using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Get
{
    internal static IEnumerable<string> Properties(ClassDeclarationSyntax classDeclarationSyntax, string className)
    {
        var props = new List<string>();
        
        foreach (var property in classDeclarationSyntax!.ChildNodes())
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;


            var useExisting = UsingExistingAttribute(propertyDeclaration);
            if (useExisting == null)
            {
                if (propertyDeclaration.ExpressionBody is not null)
                {
                    var prop = SyntaxFactory.PropertyDeclaration(propertyDeclaration.Type, identifier: propertyDeclaration.Identifier).WithModifiers(propertyDeclaration.Modifiers).WithExpressionBody(propertyDeclaration.ExpressionBody).WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    props.Add($"{prop}");
                }
                else if (propertyDeclaration.AccessorList == null ||
                         propertyDeclaration.AccessorList.Accessors.All(a =>
                             a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                    // do nothing
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
                var usingSubstitute = UsingArgumentSubstitute(useExisting);
                var replace = UsingArgumentReplace(useExisting, className);
                
                var dto = propertyDeclaration.ToString();
                if (dto.Contains("<"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = dto.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    var endIndex = dto.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = dto.Substring(startIndex + 1, endIndex - startIndex - 1);
                    if (usingSubstitute is not null)
                    {
                        dto = dto.GetLastPart("]").ReplaceFirst($"<{type}>", $"<{usingSubstitute}>");
                    }
                    else if (replace is not null)
                    {
                        dto = dto.GetLastPart("]").ReplaceFirst($"<{type}>", $"<{replace}>");
                    }
                    else
                    {
                        dto = dto.GetLastPart("]").ReplaceFirst($"<{type}>", $"<{type}DTO>");
                    }
                }
                else if (dto.Contains("[]"))
                {
                    // Extract the type parameter from the original string
                    if (usingSubstitute is not null)
                    {
                        string pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                        string replacement = $"$1{usingSubstitute}[] $3 {{ get; set;}}";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);                      
                    }
                    else if (replace is not null)
                    {
                        string pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                        string replacement = $"$1{replace}[] $3 {{ get; set;}}";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);                   
                    }
                    else
                    {
                        string pattern = @"^\s*\[[^\]]*\]\s*(public\s+([A-Za-z]+)\[\]\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*})";
                        string replacement = "public $2DTO[] $3 { get; set; }";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);
                    } 
                }
                else
                {
                    dto = dto.GetLastPart("]").ReplaceFirst(
                        propertyDeclaration.Type.ToString(),
                        usingSubstitute ?? replace ?? $"{propertyDeclaration.Type}DTO");
                }
                var prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                props.Add($"{prop}");
            }
        }

        return props;
    }

    internal static string? UsingArgumentReplace(AttributeSyntax usingSyntax, string className)
    {
        var argument = AttributeArguments(usingSyntax)
            .Where(u => u.StartsWith(className) && u.Contains(" > "));
        return argument.FirstOrDefault()?.Split('>').LastOrDefault()?.Trim();
    }
    
    internal static string? UsingArgumentSubstitute(AttributeSyntax usingSyntax)
    {
        var argument = AttributeArguments(usingSyntax)
            .Where(u=> !u.Contains(" > "));
        return argument.FirstOrDefault()?.Trim();
    }
    
    internal static List<string> AttributeArguments(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList is null)
        {
            return new List<string>();
        }

        var arguments = attribute.ArgumentList.Arguments
            .Select(s => s.NormalizeWhitespace().ToFullString().Replace("\"", "")).ToList();

        return arguments;
    }


    internal static AttributeSyntax? UsingExistingAttribute(MemberDeclarationSyntax property)
    {
        return property.AttributeLists
            .SelectMany(a => a.Attributes)
            .FirstOrDefault(a => a.Name.ToString() == nameof(UseExistingDto));
    }

    internal static string AttributeName(NameSyntax? name) =>
        (name switch
        {
            SimpleNameSyntax simpleNameSyntax => simpleNameSyntax.Identifier.Text,
            QualifiedNameSyntax qualifiedNameSyntax => qualifiedNameSyntax.Right.Identifier.Text,
            _ => null
        })!;
}