using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

internal static class Map
{
    internal static List<string> FromProperties(SyntaxNode? classDeclarationSyntax, string className)
    {
        var props = new List<string>();
        foreach (var property in classDeclarationSyntax!.ChildNodes())
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;

            var propertyName = propertyDeclaration.Identifier.Text;
            var useExisting = Get.UsingExistingAttribute(propertyDeclaration);
            if (useExisting == null)
            {
                if (propertyDeclaration.AccessorList == null ||
                    propertyDeclaration.AccessorList.Accessors.All(a => a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                }
                else
                {
                    props.Add(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,SyntaxFactory.IdentifierName(propertyName),SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,SyntaxFactory.IdentifierName("instance"),SyntaxFactory.IdentifierName(propertyName)))).ToString());
                }
            }
            else
            {
                var replace = Get.UsingArgument(useExisting, className);
                var name = replace ?? $"{propertyDeclaration.Type}DTO";
                if (name.Contains("<") && name.Contains("DTO"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = name.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    var endIndex = name.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = name.Substring(startIndex + 1, endIndex - startIndex - 1);

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
    
    internal static List<string> ToProperties(SyntaxNode? classDeclarationSyntax, string className)
    {
        var props = new List<string>();
        foreach (var property in classDeclarationSyntax!.ChildNodes())
        {
            if (property is not PropertyDeclarationSyntax propertyDeclaration) continue;

            var propertyName = propertyDeclaration.Identifier.Text;
            var useExisting = Get.UsingExistingAttribute(propertyDeclaration);
            if (useExisting == null)
            {
                if (propertyDeclaration.AccessorList == null ||
                    propertyDeclaration.AccessorList.Accessors.All(a => a.Kind() != SyntaxKind.SetAccessorDeclaration))
                {
                }
                else
                {
                    var propAssignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        SyntaxFactory.IdentifierName(propertyName),
                        SyntaxFactory.IdentifierName(propertyName));

                    props.Add($"{propAssignment.ToFullString()},");      
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
                var mapToMethod = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.IdentifierName("MapTo"));

                var propAssignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.IdentifierName(propertyName),
                    SyntaxFactory.InvocationExpression(mapToMethod));

                props.Add(
                    $"{propAssignment},");
            }
        }

        return props;
    }
}