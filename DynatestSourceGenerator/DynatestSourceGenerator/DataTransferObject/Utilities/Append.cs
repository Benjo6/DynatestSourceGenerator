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
}