using DynatestSourceGenerator.Abstractions.Attributes;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Get
{
    internal static IEnumerable<string> Properties(SyntaxNode? classDeclarationSyntax, string className)
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
                    var prop = SyntaxFactory.PropertyDeclaration(type: propertyDeclaration.Type, identifier: propertyDeclaration.Identifier).WithModifiers(propertyDeclaration.Modifiers).WithAccessorList(propertyDeclaration.AccessorList).WithAttributeLists(propertyDeclaration.AttributeLists);
                    props.Add($"{prop}");
                }
                else
                {
                    props.Add($"{propertyDeclaration}");
                }
            }
            else
            {
                var replace = UsingArgument(useExisting, className);
                var dto = propertyDeclaration.ToString().GetLastPart("]").ReplaceFirst(
                    propertyDeclaration.Type.ToString(),
                    replace ?? $"{propertyDeclaration.Type}DTO");
                
                
                if (dto.Contains("<") && dto.Contains("DTO"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = dto.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    var endIndex = dto.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = dto.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Replace the type parameter with the modified type
                    dto = dto.ReplaceFirst($"<{type}>DTO", $"<{type}DTO>");
                }
                else if(dto.Contains("<"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = dto.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    var endIndex = dto.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = dto.Substring(startIndex + 1, endIndex - startIndex - 1);

                    // Replace the type parameter with the modified type
                    dto = dto.GetLastPart("]").ReplaceFirst(
                        $"<{type}>", $"<{replace}>");
                }

                var prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                props.Add($"{prop}");
            }
        }

        return props;
    }

    internal static string? UsingArgument(AttributeSyntax usingSyntax, string className)
    {
        var argument = AttributeArguments(usingSyntax)
            .Where(u => u.StartsWith(className) && u.Contains(" > "));
        return argument.FirstOrDefault()?.Split('>').LastOrDefault()?.Trim();
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
}