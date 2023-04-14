using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


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
                    props.Add(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("target"),
                        IdentifierName(propertyName)),
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, IdentifierName("instance"),
                            IdentifierName(propertyName)))).ToString());
                }
            }
            else
            {
                var replace = Get.UsingArgumentReplace(useExisting, className);
                var usingSubstitute = Get.UsingArgumentSubstitute(useExisting);
                var name = propertyDeclaration.Type.ToString();
                if (name.Contains("<"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = name.IndexOf("<", StringComparison.Ordinal);

                    // Find the index of the closing '>' character in the string
                    var endIndex = name.IndexOf(">", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = name.Substring(startIndex + 1, endIndex - startIndex - 1);

                    if (usingSubstitute is not null)
                    {
                        name = $"{usingSubstitute}";
                    }
                    else if (replace is not null)
                    {
                        name = $"{replace}";
                    }
                    else
                    {
                        // Replace the type parameter with the modified type
                        name = $"{type}DTO";
                    }

                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = instance.{propertyDeclaration.Identifier}?.Select(t=>new {name}().MapFrom(t)).ToList();");
                    continue;
                }

                if (name.Contains("[]"))
                {
                    // Find the index of the first '<' character in the string
                    var startIndex = name.IndexOf("[", StringComparison.Ordinal);

                    // Extract the type parameter from the original string
                    var type = name.Remove(startIndex);

                    if (usingSubstitute is not null)
                    {
                        name = $"{usingSubstitute}";
                    }
                    else if (replace is not null)
                    {
                        name = $"{replace}";
                    }
                    else
                    {
                        // Replace the type parameter with the modified type
                        name = $"{type}DTO";
                    }

                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = instance.{propertyDeclaration.Identifier}?.Select(t=>new {name}().MapFrom(t)).ToArray();");
                    continue;
                }

                name = usingSubstitute ?? replace ?? $"{propertyDeclaration.Type}DTO";

                props.Add(
                    $"target.{propertyDeclaration.Identifier} = new {name}().MapFrom(instance.{propertyDeclaration.Identifier});");
            }
        }

        return props;
    }

    internal static List<string> ToProperties(SyntaxNode? classDeclarationSyntax)
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
                    var propAssignment = ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("target"),
                                IdentifierName(propertyName)),
                            IdentifierName(propertyName))).NormalizeWhitespace();

                    props.Add($"{propAssignment.ToFullString()}");
                }
            }
            else
            {
                if (propertyDeclaration.ToString().Contains("<"))
                {
                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {propertyDeclaration.Identifier}.Select(t=>t.MapTo()).ToList();");
                    continue;
                }

                if (propertyDeclaration.ToString().Contains("[]"))
                {
                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {propertyDeclaration.Identifier}.ToArray();");
                    continue;
                }

                var mapToMethod = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(propertyName),
                    IdentifierName("MapTo"));

                var propAssignment = AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(propertyName),
                    InvocationExpression(mapToMethod));
                

                props.Add(
                    $"{propAssignment};");
            }
        }

        return props;
    }

    internal static void ToArrayMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToArrayMethod = LocalFunctionStatement(
                ArrayType(
                        IdentifierName(target))
                    .WithRankSpecifiers(
                        SingletonList<ArrayRankSpecifierSyntax>(
                            ArrayRankSpecifier(
                                    SingletonSeparatedList<ExpressionSyntax>(
                                        OmittedArraySizeExpression()))
                                .WithCloseBracketToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.CloseBracketToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapToArray"))
            .WithModifiers(
                TokenList(
                    Token(
                        TriviaList(
                            Whitespace("    ")),
                        SyntaxKind.PublicKeyword,
                        TriviaList(
                            Space))))
            .WithParameterList(
                ParameterList(
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("source"))
                                .WithType(
                                    ArrayType(
                                            IdentifierName(source))
                                        .WithRankSpecifiers(
                                            SingletonList<ArrayRankSpecifierSyntax>(
                                                ArrayRankSpecifier(
                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                            OmittedArraySizeExpression()))
                                                    .WithCloseBracketToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.CloseBracketToken,
                                                            TriviaList(
                                                                Space))))))))
                    .WithCloseParenToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.CloseParenToken,
                            TriviaList(
                                CarriageReturnLineFeed))))
            .WithBody(
                Block(
                        LocalDeclarationStatement(
                                VariableDeclaration(
                                        IdentifierName(
                                            Identifier(
                                                TriviaList(
                                                    Whitespace("        ")),
                                                SyntaxKind.VarKeyword,
                                                "var",
                                                "var",
                                                TriviaList(
                                                    Space))))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                    Identifier(
                                                        TriviaList(),
                                                        "target",
                                                        TriviaList(
                                                            Space)))
                                                .WithInitializer(
                                                    EqualsValueClause(
                                                            ArrayCreationExpression(
                                                                    ArrayType(
                                                                            IdentifierName(target))
                                                                        .WithRankSpecifiers(
                                                                            SingletonList<ArrayRankSpecifierSyntax>(
                                                                                ArrayRankSpecifier(
                                                                                    SingletonSeparatedList<
                                                                                        ExpressionSyntax>(
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            IdentifierName("source"),
                                                                                            IdentifierName(
                                                                                                "Length")))))))
                                                                .WithNewKeyword(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.NewKeyword,
                                                                        TriviaList(
                                                                            Space))))
                                                        .WithEqualsToken(
                                                            Token(
                                                                TriviaList(),
                                                                SyntaxKind.EqualsToken,
                                                                TriviaList(
                                                                    Space)))))))
                            .WithSemicolonToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.SemicolonToken,
                                    TriviaList(
                                        CarriageReturnLineFeed))),
                        ForStatement(
                                Block(
                                        SingletonList<StatementSyntax>(
                                            ExpressionStatement(
                                                    AssignmentExpression(
                                                            SyntaxKind.SimpleAssignmentExpression,
                                                            ElementAccessExpression(
                                                                    IdentifierName(
                                                                        Identifier(
                                                                            TriviaList(
                                                                                Whitespace("            ")),
                                                                            "target",
                                                                            TriviaList())))
                                                                .WithArgumentList(
                                                                    BracketedArgumentList(
                                                                            SingletonSeparatedList<ArgumentSyntax>(
                                                                                Argument(
                                                                                    IdentifierName("i"))))
                                                                        .WithCloseBracketToken(
                                                                            Token(
                                                                                TriviaList(),
                                                                                SyntaxKind.CloseBracketToken,
                                                                                TriviaList(
                                                                                    Space)))),
                                                            InvocationExpression(
                                                                MemberAccessExpression(
                                                                    SyntaxKind.SimpleMemberAccessExpression,
                                                                    ElementAccessExpression(
                                                                            IdentifierName("source"))
                                                                        .WithArgumentList(
                                                                            BracketedArgumentList(
                                                                                SingletonSeparatedList<ArgumentSyntax>(
                                                                                    Argument(
                                                                                        IdentifierName("i"))))),
                                                                    IdentifierName("MapTo"))))
                                                        .WithOperatorToken(
                                                            Token(
                                                                TriviaList(),
                                                                SyntaxKind.EqualsToken,
                                                                TriviaList(
                                                                    Space))))
                                                .WithSemicolonToken(
                                                    Token(
                                                        TriviaList(),
                                                        SyntaxKind.SemicolonToken,
                                                        TriviaList(
                                                            CarriageReturnLineFeed)))))
                                    .WithOpenBraceToken(
                                        Token(
                                            TriviaList(
                                                Whitespace("        ")),
                                            SyntaxKind.OpenBraceToken,
                                            TriviaList(
                                                CarriageReturnLineFeed)))
                                    .WithCloseBraceToken(
                                        Token(
                                            TriviaList(
                                                Whitespace("        ")),
                                            SyntaxKind.CloseBraceToken,
                                            TriviaList(
                                                CarriageReturnLineFeed))))
                            .WithForKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForKeyword,
                                    TriviaList(
                                        Space)))
                            .WithDeclaration(
                                VariableDeclaration(
                                        IdentifierName(
                                            Identifier(
                                                TriviaList(),
                                                SyntaxKind.VarKeyword,
                                                "var",
                                                "var",
                                                TriviaList(
                                                    Space))))
                                    .WithVariables(
                                        SingletonSeparatedList<VariableDeclaratorSyntax>(
                                            VariableDeclarator(
                                                    Identifier(
                                                        TriviaList(),
                                                        "i",
                                                        TriviaList(
                                                            Space)))
                                                .WithInitializer(
                                                    EqualsValueClause(
                                                            LiteralExpression(
                                                                SyntaxKind.NumericLiteralExpression,
                                                                Literal(0)))
                                                        .WithEqualsToken(
                                                            Token(
                                                                TriviaList(),
                                                                SyntaxKind.EqualsToken,
                                                                TriviaList(
                                                                    Space)))))))
                            .WithFirstSemicolonToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.SemicolonToken,
                                    TriviaList(
                                        Space)))
                            .WithCondition(
                                BinaryExpression(
                                        SyntaxKind.LessThanExpression,
                                        IdentifierName(
                                            Identifier(
                                                TriviaList(),
                                                "i",
                                                TriviaList(
                                                    Space))),
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("source"),
                                            IdentifierName("Length")))
                                    .WithOperatorToken(
                                        Token(
                                            TriviaList(),
                                            SyntaxKind.LessThanToken,
                                            TriviaList(
                                                Space))))
                            .WithSecondSemicolonToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.SemicolonToken,
                                    TriviaList(
                                        Space)))
                            .WithIncrementors(
                                SingletonSeparatedList<ExpressionSyntax>(
                                    PostfixUnaryExpression(
                                        SyntaxKind.PostIncrementExpression,
                                        IdentifierName("i"))))
                            .WithCloseParenToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.CloseParenToken,
                                    TriviaList(
                                        CarriageReturnLineFeed))),
                        ReturnStatement(
                                IdentifierName("target"))
                            .WithReturnKeyword(
                                Token(
                                    TriviaList(
                                        new[]
                                        {
                                            CarriageReturnLineFeed,
                                            Whitespace("        ")
                                        }),
                                    SyntaxKind.ReturnKeyword,
                                    TriviaList(
                                        Space)))
                            .WithSemicolonToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.SemicolonToken,
                                    TriviaList(
                                        CarriageReturnLineFeed))))
                    .WithOpenBraceToken(
                        Token(
                            TriviaList(
                                Whitespace("    ")),
                            SyntaxKind.OpenBraceToken,
                            TriviaList(
                                CarriageReturnLineFeed)))
                    .WithCloseBraceToken(
                        Token(
                            TriviaList(
                                Whitespace("    ")),
                            SyntaxKind.CloseBraceToken,
                            TriviaList())));

        classBuilder.Append($"\t{mapToArrayMethod}");
    }


    internal static string FromArrayMethod(StringBuilder classBuilder,string target, string source)
    {
        throw new NotImplementedException();
    }
}