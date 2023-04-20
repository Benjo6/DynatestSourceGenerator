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
                switch (nameof(property.Type))
                {
                    case string s when s == nameof(Types.IDictionaryT) || s == nameof(Types.IReadOnlyDictionaryT) || s == nameof(Types.KeyValuePairT) || s == nameof(Types.DictionaryT) || s == nameof(Types.IImmutableDictionaryT) || s == nameof(Types.ImmutableDictionaryT):
                        // Handle IDictionary or IReadOnlyDictionary, Handle KeyValuePair<TKey, TValue>, Dictionary<TKey, TValue>, IImmutableDictionary or ImmutableDictionary<TKey, TValue>
                        dto = Append.AppropriateDataTransferObjectNameDictionary(dto, usingSubstitute, replace);
                        break;
                    
                    case string s when s == nameof(Types.IEnumerableT) || s == nameof(Types.ICollectionT) || s == nameof(Types.IReadOnlyCollectionT) || s == nameof(Types.ListT) || s == nameof(Types.IListT) || s == nameof(Types.StackT) || s == nameof(Types.QueueT) || s == nameof(Types.IReadOnlyListT) || s == nameof(Types.IQueryableT) || s == nameof(Types.ImmutableArrayT) ||  s == nameof(Types.ImmutableListT) || s == nameof(Types.ImmutableHashSetT) || s == nameof(Types.ImmutableQueueT) || s == nameof(Types.ImmutableStackT) || s == nameof(Types.ImmutableSortedSetT):
                        // Handle IEnumerable, ICollection<T>, IReadOnlyCollection<T>, IList, List<T>, Stack<T>, Queue<T>, IReadOnlyList<T>, IQueryable<T>, ImmutableArray<T>, ImmutableList<T>, ImmutableHashSet<T>,  ImmutableQueue<T>, ImmutableStack<T>, ImmutableSortedSet<T>
                        dto = Append.AppropriateDataTransferObjectNameList(dto, usingSubstitute, replace);
                        break;
                    
                    case string s when s.Contains("[]"):
                        //Handle Array
                        if (usingSubstitute is not null)
                        {
                            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                            var replacement = $"$1{usingSubstitute}[] $3 {{ get; set;}}";
                            dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);                      
                        }
                        else if (replace is not null)
                        {
                            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
                            var replacement = $"$1{replace}[] $3 {{ get; set;}}";
                            dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);                 
                        }
                        else
                        {
                            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+([A-Za-z]+)\[\]\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*})";
                            var replacement = "public $2DTO[] $3 { get; set; }";
                            dto = Regex.Replace(dto, pattern, replacement, RegexOptions.Multiline);
                        } 
                        break;
                    default:
                        dto = dto.GetLastPart("]").ReplaceFirst(
                            property.Type.ToString(),
                            usingSubstitute ?? replace ?? $"{property.Type}DTO");
                        break;
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
}