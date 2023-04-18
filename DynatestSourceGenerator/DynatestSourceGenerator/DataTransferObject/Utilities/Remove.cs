using System;
using System.Linq;
using DynatestSourceGenerator.Abstractions.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Remove
{
    internal static ClassDeclarationSyntax? ExcludedProperties(ClassDeclarationSyntax classDeclaration,
        string className)
    {
        var ignoredProperties = classDeclaration.ChildNodes()
            .OfType<PropertyDeclarationSyntax>()
            .Where(p => p.AttributeLists.SelectMany(a => a.Attributes)
                .Any(a => a.Name.ToString() == nameof(ExcludeProperty) &&
                          (!Get.AttributeArguments(a).Any() || Get.AttributeArguments(a).Contains(className))));

        var getClassWithoutIgnoredProperties =
            classDeclaration.RemoveNodes(ignoredProperties, SyntaxRemoveOptions.KeepEndOfLine);

        return getClassWithoutIgnoredProperties;
    }
    
    internal static string ArrayBrackets(string name)
    {
        // Find the index of the first '<' character in the string
        var startIndex = name.IndexOf("[", StringComparison.Ordinal);

        // Extract the type parameter from the original string
        var type = name.Remove(startIndex);
        return type;
    }
}