using System;
using System.Linq;
using System.Text;
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
    internal static string AppropriateDataTransferObjectNameList(string name, string? substituteName, string? replacementName)
    {
        // Find the index of the first '<' character in the string
        var startIndex = name.IndexOf("<", StringComparison.Ordinal);

        // Find the index of the closing '>' character in the string
        var endIndex = name.IndexOf(">", StringComparison.Ordinal);

        // Extract the type parameter from the original string
        var type = name.Substring(startIndex + 1, endIndex - startIndex - 1);

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
            name = $"{type}DTO";
        }

        return name;
    }
    internal static string AppropriateDataTransferObjectName(string name, string? substituteName, string? replacementName)
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
}