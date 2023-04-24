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
                            IdentifierName(propertyName)))).NormalizeWhitespace().ToString());
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
                    name = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {name}.MapFromList(instance.{propertyDeclaration.Identifier});");
                    continue;
                }

                if (name.Contains("[]"))
                {
                    var type = Remove.ArrayBrackets(name);
                    name = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {name}.MapFromArray(instance.{propertyDeclaration.Identifier});");
                    continue;
                }

                name = usingSubstitute ?? replace ?? $"{propertyDeclaration.Type}DTO";

                props.Add(
                    $"target.{propertyDeclaration.Identifier} = new {name}().MapFrom(instance.{propertyDeclaration.Identifier});");
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
                    var propAssignment = ExpressionStatement(
                        AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("target"),
                                IdentifierName(propertyName)),
                            IdentifierName(propertyName))).NormalizeWhitespace();

                    props.Add($"{propAssignment}");
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
                    name = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {name}.MapToList({propertyDeclaration.Identifier});");
                    continue;
                }

                if (name.Contains("[]"))
                {
                    name = Remove.ArrayBrackets(name);
                    name = Append.AppropriateDataTransferObjectName(name, usingSubstitute, replace);
                    props.Add(
                        $"target.{propertyDeclaration.Identifier} = {name}.MapToArray({propertyDeclaration.Identifier});");
                    continue;
                }

                var propAssignment = ExpressionStatement(
                    AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("target"),
                            IdentifierName(propertyName)),
                        InvocationExpression(
                            MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(propertyName),
                                IdentifierName("MapTo"))))).NormalizeWhitespace();



                props.Add(
                    $"{propAssignment}");
            }
        }

        return props;
    }

    internal static void ToEnumerableMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToListMethod = LocalFunctionStatement(
                GenericName(
                    Identifier("List"))
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            IdentifierName(target)))
                    .WithGreaterThanToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.GreaterThanToken,
                            TriviaList(
                                Space)))),
                Identifier("MapToList"))
            .WithModifiers(
                TokenList(Token(
                        TriviaList(
                            Whitespace("    ")),
                        SyntaxKind.InternalKeyword,
                        TriviaList(
                            Space)), Token(
                        TriviaList(),
                        SyntaxKind.StaticKeyword,
                        TriviaList(
                            Space))))
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                            Identifier("source"))
                        .WithType(
                            GenericName(
                                Identifier("IEnumerable"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(source)))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space)))))))
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
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(
                                        TriviaList(),
                                        "sourceList",
                                        TriviaList(
                                            Space)))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("List"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(source)))))
                                        .WithNewKeyword(
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.NewKeyword,
                                                TriviaList(
                                                    Space)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        IdentifierName("source"))))))
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
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(
                                        TriviaList(),
                                        "target",
                                        TriviaList(
                                            Space)))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("List"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(target)))))
                                        .WithNewKeyword(
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.NewKeyword,
                                                TriviaList(
                                                    Space)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("sourceList"),
                                                            IdentifierName("Count")))))))
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
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(
                                                        Whitespace("            ")),
                                                    "target",
                                                    TriviaList())),
                                            IdentifierName("Add")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            ElementAccessExpression(
                                                                IdentifierName("sourceList"))
                                                            .WithArgumentList(
                                                                BracketedArgumentList(
                                                                    SingletonSeparatedList(
                                                                        Argument(
                                                                            IdentifierName("i"))))),
                                                            IdentifierName("MapTo"))))))))
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
                            SingletonSeparatedList(
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
                                IdentifierName("sourceList"),
                                IdentifierName("Count")))
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
                                Whitespace("        ")),
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
        classBuilder.Append($"\t{mapToListMethod}\n");

    }

    internal static void ToArrayMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToArrayMethod = LocalFunctionStatement(
                ArrayType(
                        IdentifierName(target))
                    .WithRankSpecifiers(
                        SingletonList(
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
                TokenList(Token(
                    TriviaList(
                        Whitespace("    ")),
                    SyntaxKind.InternalKeyword,
                    TriviaList(
                        Space)), Token(
                    TriviaList(),
                    SyntaxKind.StaticKeyword,
                    TriviaList(
                        Space))))
            .WithParameterList(
                ParameterList(
                        SingletonSeparatedList(
                            Parameter(
                                    Identifier("source"))
                                .WithType(
                                    ArrayType(
                                            IdentifierName(source))
                                        .WithRankSpecifiers(
                                            SingletonList(
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
                                        SingletonSeparatedList(
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
                                                                            SingletonList(
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
                                                                            SingletonSeparatedList(
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
                                                                                SingletonSeparatedList(
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
                                        SingletonSeparatedList(
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
                                    TriviaList(CarriageReturnLineFeed, Whitespace("        ")),
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

        classBuilder.Append($"\t{mapToArrayMethod}\n");
    }


    internal static void FromEnumerableMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapFromListMethod = LocalFunctionStatement(
                GenericName(
                    Identifier("List"))
                .WithTypeArgumentList(
                    TypeArgumentList(
                        SingletonSeparatedList<TypeSyntax>(
                            IdentifierName(target)))
                    .WithGreaterThanToken(
                        Token(
                            TriviaList(),
                            SyntaxKind.GreaterThanToken,
                            TriviaList(
                                Space)))),
                Identifier("MapFromList"))
            .WithModifiers(
                TokenList(Token(
                        TriviaList(
                            Whitespace("    ")),
                        SyntaxKind.InternalKeyword,
                        TriviaList(
                            Space)), Token(
                        TriviaList(),
                        SyntaxKind.StaticKeyword,
                        TriviaList(
                            Space))))
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                            Identifier("source"))
                        .WithType(
                            GenericName(
                                Identifier("IEnumerable"))
                            .WithTypeArgumentList(
                                TypeArgumentList(
                                    SingletonSeparatedList<TypeSyntax>(
                                        IdentifierName(source)))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space)))))))
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
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(
                                        TriviaList(),
                                        "sourceList",
                                        TriviaList(
                                            Space)))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("List"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(source)))))
                                        .WithNewKeyword(
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.NewKeyword,
                                                TriviaList(
                                                    Space)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        IdentifierName("source"))))))
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
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(
                                        TriviaList(),
                                        "target",
                                        TriviaList(
                                            Space)))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                            GenericName(
                                                Identifier("List"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                    SingletonSeparatedList<TypeSyntax>(
                                                        IdentifierName(target)))))
                                        .WithNewKeyword(
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.NewKeyword,
                                                TriviaList(
                                                    Space)))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            IdentifierName("sourceList"),
                                                            IdentifierName("Count")))))))
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
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName(
                                                Identifier(
                                                    TriviaList(
                                                        Whitespace("            ")),
                                                    "target",
                                                    TriviaList())),
                                            IdentifierName("Add")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList(
                                                Argument(
                                                    InvocationExpression(
                                                        MemberAccessExpression(
                                                            SyntaxKind.SimpleMemberAccessExpression,
                                                            ElementAccessExpression(
                                                                IdentifierName("target"))
                                                            .WithArgumentList(
                                                                BracketedArgumentList(
                                                                    SingletonSeparatedList(
                                                                        Argument(
                                                                            IdentifierName("i"))))),
                                                            IdentifierName("MapFrom")))
                                                    .WithArgumentList(
                                                        ArgumentList(
                                                            SingletonSeparatedList(
                                                                Argument(
                                                                    ElementAccessExpression(
                                                                        IdentifierName("sourceList"))
                                                                    .WithArgumentList(
                                                                        BracketedArgumentList(
                                                                            SingletonSeparatedList(
                                                                                Argument(
                                                                                    IdentifierName("i"))))))))))))))
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
                            SingletonSeparatedList(
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
                                IdentifierName("sourceList"),
                                IdentifierName("Count")))
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
                                Whitespace("        ")),
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
        classBuilder.Append($"\t{mapFromListMethod}\n");

    }
    internal static void FromArrayMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapFromArray = LocalFunctionStatement(
                ArrayType(
                    IdentifierName(target))
                .WithRankSpecifiers(
                    SingletonList(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(
                                OmittedArraySizeExpression()))
                        .WithCloseBracketToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.CloseBracketToken,
                                TriviaList(
                                    Space))))),
                Identifier("MapFromArray"))
            .WithModifiers(
                TokenList(Token(
                        TriviaList(
                            Whitespace("    ")),
                        SyntaxKind.InternalKeyword,
                        TriviaList(
                            Space)), Token(
                        TriviaList(),
                        SyntaxKind.StaticKeyword,
                        TriviaList(
                            Space))))
            .WithParameterList(
                ParameterList(
                    SingletonSeparatedList(
                        Parameter(
                            Identifier("source"))
                        .WithType(
                            ArrayType(
                                IdentifierName(source))
                            .WithRankSpecifiers(
                                SingletonList(
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
                            SingletonSeparatedList(
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
                                                SingletonList(
                                                    ArrayRankSpecifier(
                                                        SingletonSeparatedList<ExpressionSyntax>(
                                                            MemberAccessExpression(
                                                                SyntaxKind.SimpleMemberAccessExpression,
                                                                IdentifierName("source"),
                                                                IdentifierName("Length")))))))
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
                                                SingletonSeparatedList(
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
                                                    IdentifierName("target"))
                                                .WithArgumentList(
                                                    BracketedArgumentList(
                                                        SingletonSeparatedList(
                                                            Argument(
                                                                IdentifierName("i"))))),
                                                IdentifierName("MapFrom")))
                                        .WithArgumentList(
                                            ArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(
                                                        ElementAccessExpression(
                                                            IdentifierName("source"))
                                                        .WithArgumentList(
                                                            BracketedArgumentList(
                                                                SingletonSeparatedList(
                                                                    Argument(
                                                                        IdentifierName("i"))))))))))
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
                            SingletonSeparatedList(
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
                            TriviaList(CarriageReturnLineFeed, Whitespace("        ")),
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
        classBuilder.Append($"\t{mapFromArray}\n");
    }

}