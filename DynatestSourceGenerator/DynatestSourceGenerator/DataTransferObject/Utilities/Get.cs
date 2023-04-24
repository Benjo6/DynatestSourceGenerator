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

        foreach (var property in classDeclarationSyntax!.Members.OfType<PropertyDeclarationSyntax>())
        {
            var useExisting = UsingExistingAttribute(property);
            if (useExisting is null)
            {
                if (property.ExpressionBody is not null)
                {
                    var prop = SyntaxFactory.PropertyDeclaration(property.Type, property.Identifier)
                        .WithModifiers(property.Modifiers)
                        .WithExpressionBody(property.ExpressionBody)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                    props.Add(prop.ToString());
                }
                else if (property.AccessorList == null ||
                         property.AccessorList.Accessors.All(a =>
                             a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                    // do nothing
                }
                else if (property.ToString().StartsWith("["))
                {
                    var prop1 = property.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
                    props.Add($"{prop1.First()}\r\n\t{prop1.Last().TrimStart()}");
                }
                else
                {
                    props.Add($"{property.ToString()}");
                }
            }
            else
            {
                var usingSubstitute = UsingArgumentSubstitute(useExisting);
                var replace = UsingArgumentReplace(useExisting, className);
                var dto = property.ToString();
                if (property.Type is ArrayTypeSyntax)
                {
                    if (usingSubstitute is not null)
                    {
                        var pattern =
                            @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                        var replacement = $"$1{usingSubstitute}[] $3 {{ get; set;}}";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);
                    }
                    else if (replace is not null)
                    {
                        var pattern = @"^\s*\[[^\]]*\]\s*(public\s+([A-Za-z]+)\[\]\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                        var replacement = $"$1{replace}[] $3 {{ get; set;}}";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);
                    }
                    else
                    {
                        var pattern =
                            @"^\s*\[[^\]]*\]\s*(public\s+([A-Za-z]+)\[\]\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*})";
                        var replacement = "public $2DTO[] $3 { get; set; }";
                        dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);
                    }
                    var prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                    props.Add($"{prop}");
                }
                else if (property.Type is GenericNameSyntax genericNameSyntax)
                {
                    MemberDeclarationSyntax? prop;
                    switch (genericNameSyntax.Identifier.Text)
                    {
                        case Types.IDictionaryT:
                        case Types.DictionaryT:
                        case Types.IReadOnlyDictionaryT:
                        case Types.KeyValuePairT:
                        case Types.IImmutableDictionaryT:
                        case Types.ImmutableDictionaryT:
                        case Types.ImmutableSortedDictionaryT:
                            dto = Append.AppropriateDataTransferObjectNameDictionary(dto, usingSubstitute, replace);
                            dto = dto.GetLastPart("]");
                            prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                            props.Add($"{prop}");
                            break;
                        case Types.IEnumerableT:
                        case Types.ICollectionT:
                        case Types.IReadOnlyCollectionT:
                        case Types.IListT:
                        case Types.ListT:
                        case Types.StackT:
                        case Types.QueueT:
                        case Types.IReadOnlyListT:
                        case Types.IQueryableT:
                        case Types.ImmutableArrayT:
                        case Types.ImmutableListT:
                        case Types.IImmutableListT:
                        case Types.ImmutableHashSetT:
                        case Types.IImmutableSetT:
                        case Types.ImmutableQueueT:
                        case Types.IImmutableQueueT:
                        case Types.ImmutableStackT:
                        case Types.IImmutableStackT:
                        case Types.ImmutableSortedSetT:
                            // Find the index of the first '<' character in the string
                            var startIndex = dto.IndexOf("<", StringComparison.Ordinal);

                            // Find the index of the closing '>' character in the string
                            var endIndex = dto.IndexOf(">", StringComparison.Ordinal);

                            // Extract the type parameter from the original string
                            var type = dto.Substring(startIndex + 1, endIndex - startIndex - 1);
                            dto = dto.GetLastPart("]").ReplaceFirst(type,
                                Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace));
                            prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                            props.Add($"{prop}");
                            break;
                    }
                }
                else if (property.Type is SimpleNameSyntax)
                {
                    dto = dto.GetLastPart("]").ReplaceFirst(
                        property.Type.ToString(),
                        usingSubstitute ?? replace ?? $"{property.Type}DTO");
                    var prop = SyntaxFactory.ParseMemberDeclaration($"{dto.TrimStart()}");
                    props.Add($"{prop}");
                }
                else
                {
                    continue;
                }
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
            .Where(u => !u.Contains(" > "));
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
    
    public static bool Match(TypeSyntax type, TypeSyntax otherType)
    {
        if (type == null || otherType == null)
            return false;
        
        if (type.IsEquivalentTo(otherType))
            return true;

        if (type is GenericNameSyntax genericType && otherType is GenericNameSyntax otherGenericType)
        {
            if (genericType.Identifier.ValueText != otherGenericType.Identifier.ValueText)
                return false;

            var typeArguments = genericType.TypeArgumentList.Arguments;
            var otherTypeArguments = otherGenericType.TypeArgumentList.Arguments;
            if (typeArguments.Count != otherTypeArguments.Count)
                return false;

            for (int i = 0; i < typeArguments.Count; i++)
            {
                if (!Match(typeArguments[i], otherTypeArguments[i]))
                    return false;
            }

            return true;
        }

        return false;
    }
}