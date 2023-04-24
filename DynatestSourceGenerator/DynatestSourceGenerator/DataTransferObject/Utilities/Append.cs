using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Append
{
    internal static void NamespacesToFile(ClassDeclarationSyntax classDeclarationSyntax, StringBuilder classBuilder)
    {
        if (Directives.Namespace(classDeclarationSyntax) != null && Directives.Namespace(classDeclarationSyntax).Any())
        {
            foreach (var namespaceDirective in Directives.Namespace(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name};");
            }
        }
        else
        {
            foreach (var namespaceDirective in Directives.FileScopedNamespace(classDeclarationSyntax))
            {
                classBuilder.AppendLine($"using {namespaceDirective!.Name};");
            }
        }
    }

    internal static string AppropriateDataTransferObjectNameArray(string name, string? substituteName,
        string? replacementName)
    {
        // Extract the type parameter from the original string
        if (substituteName is not null)
        {
            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
            var replacement = $"$1{substituteName}[] $3 {{ get; set;}}";
            name = Regex.Replace(name, pattern, replacement, RegexOptions.Multiline);
        }
        else if (replacementName is not null)
        {
            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+)([A-Za-z]+\[\])\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*}";
            var replacement = $"$1{replacementName}[] $3 {{ get; set;}}";
            name = Regex.Replace(name, pattern, replacement, RegexOptions.Multiline);
        }
        else
        {
            var pattern = @"^\s*\[[^\]]*\]\s*(public\s+([A-Za-z]+)\[\]\s+([A-Za-z]+)\s*{\s*get;\s*set;\s*})";
            var replacement = "public $2DTO[] $3 { get; set; }";
            name = Regex.Replace(name, pattern, replacement, RegexOptions.Multiline);
        }

        return name;
    }
    
    internal static string AppropriateDataTransferObjectNameDictionary(string name, string? substituteName,
        string? replacementName)
    {
        // Define a regular expression pattern to match the type parameter in the input string
        var pattern = @"(?<=<)\s*(?<keyType>[^,]+)\s*,\s*(?<valueType>[^>]+)\s*(?=>)";
        // Use the regular expression to find the key and value types in the input string
        var match = Regex.Match(name, pattern);

        // Extract the key and value types from the match
        var keyType = match.Groups["keyType"].Value.Trim();
        var valueType = match.Groups["valueType"].Value.Trim();
        valueType = substituteName ?? replacementName ?? $"{valueType}DTO";
        // Replace the original type parameter with the modified types
        name = Regex.Replace(name, pattern, $"{keyType}, {valueType}");

        return name;
    }
    internal static string AppropriateDataTransferObjectNameDicValueColumn(string name, string? substituteName,
        string? replacementName)
    {
        // Define a regular expression pattern to match the type parameter in the input string
        var pattern = @"(?<=<)\s*(?<keyType>[^,]+)\s*,\s*(?<valueType>[^>]+)\s*(?=>)";
        // Use the regular expression to find the key and value types in the input string
        var match = Regex.Match(name, pattern);

        var valueType = match.Groups["valueType"].Value.Trim();
        valueType = substituteName ?? replacementName ?? $"{valueType}DTO";

        return valueType;
    }

    internal static string AppropriateDataTransferObjectName(string name, string? substituteName,
        string? replacementName)
    {
        if (substituteName is not null)
        {
            name = $"{substituteName}";
        }
        else if (replacementName is not null)
        {
            name = $"{replacementName}";
        }
        else
        {
            // Replace the type parameter with the modified type
            name = $"{name}DTO";
        }

        return name;
    }
    private static Boolean CanCovert(String value, Type type)
    {
        TypeConverter converter = TypeDescriptor.GetConverter(type);
        return converter.IsValid(value);
    }
    
}