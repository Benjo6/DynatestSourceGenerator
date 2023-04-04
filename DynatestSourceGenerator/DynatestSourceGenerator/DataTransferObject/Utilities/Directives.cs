using System.Collections.Generic;
using System.Linq;
using DynatestSourceGenerator.DataTransferObject.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Directives
{
    internal static IEnumerable<UsingDirectiveSyntax> Using(SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as UsingDirectiveSyntax).WhereNotNull()!;
    }
    
    internal static IEnumerable<NamespaceDeclarationSyntax> Namespace(SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as NamespaceDeclarationSyntax).WhereNotNull()!;
    }
    
    internal static IEnumerable<FileScopedNamespaceDeclarationSyntax> FileScopedNamespace(
        SyntaxNode classDeclarationSyntax)
    {
        return classDeclarationSyntax.SyntaxTree.GetRoot().DescendantNodes()
            .Select(s => s as FileScopedNamespaceDeclarationSyntax).WhereNotNull()!;
    }
}