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
                var name = propertyDeclaration.Type;
                switch (name)
                {
                    case ArrayTypeSyntax:
                    {
                        var type = Remove.ArrayBrackets(name.ToString());
                        type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                        props.Add(
                            $"target.{propertyDeclaration.Identifier} = {type}.MapFromArray(instance.{propertyDeclaration.Identifier});");
                        break;
                    }
                    case GenericNameSyntax genericNameSyntax:
                    {
                        string? type;
                        int startIndex;
                        int endIndex;
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case Types.IDictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(), usingSubstitute, replace);
                                    props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                    props.Add($"{{");
                                    props.Add(
                                        $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromIDictionary(instance.{propertyDeclaration.Identifier});");
                                    props.Add($"}}");
                                    break;
                                case Types.DictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),usingSubstitute, replace);
                                    props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                    props.Add($"{{");
                                    props.Add(
                                        $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromDictionary(instance.{propertyDeclaration.Identifier});");
                                    props.Add($"}}");
                                    break;
                                case Types.IReadOnlyDictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromIReadOnlyDictionary(instance.{propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.KeyValuePairT:
                                props.Add(
                                    $"target.{propertyDeclaration.Identifier} = MapFromKeyValuePair(instance.{propertyDeclaration.Identifier});");
                                break;
                            case Types.IEnumerableT:
                            case Types.ICollectionT:
                            case Types.IReadOnlyCollectionT:
                            case Types.IListT:
                            case Types.ListT:
                            case Types.IReadOnlyListT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromList(instance.{propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.StackT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromStack(instance.{propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.QueueT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if (instance.{propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapFromQueue(instance.{propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.ImmutableArrayT:
                                props.Add(
                                    $"target.{propertyDeclaration.Identifier} = MapFromArray(instance.{propertyDeclaration.Identifier});");
                                break;
                        }
                        break;
                    }
                    default:
                        continue;
                }
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
                var name = propertyDeclaration.Type;
                switch (name)
                {
                    case ArrayTypeSyntax:
                    {
                        var type = Remove.ArrayBrackets(name.ToString());
                        type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                        props.Add(
                            $"target.{propertyDeclaration.Identifier} = {type}.MapToArray({propertyDeclaration.Identifier});");
                        break;
                    }
                    case GenericNameSyntax genericNameSyntax:
                    {
                        string? type;
                        int startIndex;
                        int endIndex;
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case Types.IDictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToIDictionary({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.DictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToDictionary({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.IReadOnlyDictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToIReadOnlyDictionary({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.KeyValuePairT:
                                props.Add(
                                    $"target.{propertyDeclaration.Identifier} = MapToKeyValuePair({propertyDeclaration.Identifier});");
                                break;
                            case Types.IEnumerableT:
                            case Types.ICollectionT:
                            case Types.IReadOnlyCollectionT:
                            case Types.IListT:
                            case Types.ListT:
                            case Types.IReadOnlyListT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToList({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.StackT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToStack({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.QueueT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                props.Add($"if ({propertyDeclaration.Identifier} != null)");
                                props.Add($"{{");
                                props.Add(
                                    $"\ttarget.{propertyDeclaration.Identifier} = {type}.MapToQueue({propertyDeclaration.Identifier});");
                                props.Add($"}}");
                                break;
                            case Types.ImmutableArrayT:
                                props.Add(
                                    $"target.{propertyDeclaration.Identifier} = MapToArray({propertyDeclaration.Identifier});");
                                break;
                        }

                        break;
                    }
                    default:
                        continue;
                }
            }
        }

        return props;
    }

    internal static void FromKeyValuePairMethod(ClassDeclarationSyntax ClassDeclaration, string target, string source,
        StringBuilder classBuilder)
    {
        classBuilder.AppendLine("\n");
        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.KeyValuePair<int, {target}> MapFromKeyValuePair(System.Collections.Generic.KeyValuePair<int, {source}> source)");
        classBuilder.AppendLine(
            "\t{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.KeyValuePair<int, {target}>();");
        classBuilder.AppendLine(
            $"\t\tif (source.Value != null && target.Value != null)");
        classBuilder.AppendLine(
            $"\t\t{{");
        foreach (var property in ClassDeclaration!.ChildNodes())
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


                    classBuilder.AppendLine($"\t\t\ttarget.Value.{propertyName} = source.Value.{propertyName};");
                }
            }
            else
            {
                var replace = Get.UsingArgumentReplace(useExisting, target);
                var usingSubstitute = Get.UsingArgumentSubstitute(useExisting);
                var name = propertyDeclaration.Type;
                switch (name)
                {
                    case ArrayTypeSyntax:
                    {
                        var type = Remove.ArrayBrackets(name.ToString());
                        type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                        classBuilder.AppendLine(
                            $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromArray(source.Value.{propertyDeclaration.Identifier});");
                        break;
                    }
                    case GenericNameSyntax genericNameSyntax:
                    {
                        string? type;
                        int startIndex;
                        int endIndex;
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case Types.IDictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
              usingSubstitute, replace);
                                    classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                    classBuilder.AppendLine($"\t\t\t{{");
                                    classBuilder.AppendLine(
                                        $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromIDictionary(source.Value.{propertyDeclaration.Identifier});");
                                    classBuilder.AppendLine($"\t\t\t}}");
                                    break;
                                case Types.DictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
              usingSubstitute, replace);
                                    classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                    classBuilder.AppendLine($"\t\t\t{{");
                                    classBuilder.AppendLine(
                                        $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromDictionary(source.Value.{propertyDeclaration.Identifier});");
                                    classBuilder.AppendLine($"\t\t\t}}");
                                    break;
                                case Types.IReadOnlyDictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromIReadOnlyDictionary(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.KeyValuePairT:
                                classBuilder.AppendLine(
                                    $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = MapFromKeyValuePair(source.Value.{propertyDeclaration.Identifier});");
                                break;
                            case Types.IEnumerableT:
                            case Types.ICollectionT:
                            case Types.IReadOnlyCollectionT:
                            case Types.IListT:
                            case Types.ListT:
                            case Types.IReadOnlyListT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromList(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine("\t\t\t}");
                                break;
                            case Types.StackT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromStack(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.QueueT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapFromQueue(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.ImmutableArrayT:
                                classBuilder.AppendLine(
                                    $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = MapFromImmutableArray(source.Value.{propertyDeclaration.Identifier});");
                                break;
                        }

                        break;
                    }
                    default:
                        continue;
                }
            }
        }
        classBuilder.AppendLine("\t\t}");
        classBuilder.AppendLine("\t\treturn target;");
        classBuilder.AppendLine("\t}");
    } 
    internal static void ToKeyValuePairMethod(ClassDeclarationSyntax ClassDeclaration, string target, string source,
        StringBuilder classBuilder)
    {
        classBuilder.AppendLine("\n");

        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.KeyValuePair<int, {target}> MapToKeyValuePair(System.Collections.Generic.KeyValuePair<int, {source}> source)");
        classBuilder.AppendLine(
            "\t{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.KeyValuePair<int, {target}>();");
        classBuilder.AppendLine(
            $"\t\tif (source.Value != null && target.Value != null)");
        classBuilder.AppendLine(
            $"\t\t{{");
        foreach (var property in ClassDeclaration!.ChildNodes())
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


                    classBuilder.AppendLine($"\t\t\ttarget.Value.{propertyName} = source.Value.{propertyName};");
                }
            }
            else
            {
                var replace = Get.UsingArgumentReplace(useExisting, target);
                var usingSubstitute = Get.UsingArgumentSubstitute(useExisting);
                var name = propertyDeclaration.Type;
                switch (name)
                {
                    case ArrayTypeSyntax:
                    {
                        var type = Remove.ArrayBrackets(name.ToString());
                        type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);

                        classBuilder.AppendLine(
                            $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToArray(source.Value.{propertyDeclaration.Identifier});");
                        break;
                    }
                    case GenericNameSyntax genericNameSyntax:
                    {
                        string? type;
                        int startIndex;
                        int endIndex;
                        switch (genericNameSyntax.Identifier.Text)
                        {
                            case Types.IDictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                       usingSubstitute, replace);
                                    classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                    classBuilder.AppendLine($"\t\t\t{{");
                                    classBuilder.AppendLine(
                                        $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToIDictionary(source.Value.{propertyDeclaration.Identifier});");
                                    classBuilder.AppendLine($"\t\t\t}}");
                                    break;
                                case Types.DictionaryT:
                                    type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                       usingSubstitute, replace);
                                    classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                    classBuilder.AppendLine($"\t\t\t{{");
                                    classBuilder.AppendLine(
                                        $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToDictionary(source.Value.{propertyDeclaration.Identifier});");
                                    classBuilder.AppendLine($"\t\t\t}}");
                                    break;
                                case Types.IReadOnlyDictionaryT:
                                type = Append.AppropriateDataTransferObjectNameDicValueColumn(name.ToString(),
                                    usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToIReadOnlyDictionary(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.KeyValuePairT:
                                classBuilder.AppendLine(
                                    $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = MapToKeyValuePair(source.Value.{propertyDeclaration.Identifier});");
                                break;
                            case Types.IEnumerableT:
                            case Types.ICollectionT:
                            case Types.IReadOnlyCollectionT:
                            case Types.IListT:
                            case Types.ListT:
                            case Types.IReadOnlyListT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToList(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine("\t\t\t}");
                                break;
                            case Types.StackT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToStack(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.QueueT:
                                // Find the index of the first '<' character in the string
                                startIndex = name.ToString().IndexOf("<", StringComparison.Ordinal);

                                // Find the index of the closing '>' character in the string
                                endIndex = name.ToString().IndexOf(">", StringComparison.Ordinal);

                                // Extract the type parameter from the original string
                                type = name.ToString().Substring(startIndex + 1, endIndex - startIndex - 1);
                                type = Append.AppropriateDataTransferObjectName(type, usingSubstitute, replace);
                                classBuilder.AppendLine($"\t\t\tif (source.Value.{propertyDeclaration.Identifier} != null)");
                                classBuilder.AppendLine($"\t\t\t{{");
                                classBuilder.AppendLine(
                                    $"\t\t\t\ttarget.Value.{propertyDeclaration.Identifier} = {type}.MapToQueue(source.Value.{propertyDeclaration.Identifier});");
                                classBuilder.AppendLine($"\t\t\t}}");
                                break;
                            case Types.ImmutableArrayT:
                                classBuilder.AppendLine(
                                    $"\t\t\ttarget.Value.{propertyDeclaration.Identifier} = MapToImmutableArray(source.Value.{propertyDeclaration.Identifier});");
                                break;
                        }

                        break;
                    }
                    default:
                        continue;
                }
            }
        }
        classBuilder.AppendLine("\t\t}");
        classBuilder.AppendLine("\t\treturn target;");
        classBuilder.AppendLine("\t}");
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
    internal static void ToDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("Dictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapToDictionary"))
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
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("source"))
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("Dictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
                                                                .WithNewKeyword(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.NewKeyword,
                                                                        TriviaList(
                                                                            Space)))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            IdentifierName("item"),
                                                                                            IdentifierName("Key")))))),
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapToIDictionary}\n");
    }
    internal static void FromDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapFromIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("Dictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapFromDictionary"))
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
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("Dictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
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
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                            MemberAccessExpression(
                                                                                                SyntaxKind
                                                                                                    .SimpleMemberAccessExpression,
                                                                                                IdentifierName("item"),
                                                                                                IdentifierName(
                                                                                                    "Key")))))),
                                                                        IdentifierName("MapFrom")))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("item"),
                                                                                    IdentifierName("Value")))))))
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapFromIDictionary}\n");
    }
    internal static void ToIDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("IDictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapToIDictionary"))
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
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("IDictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
                                                                .WithNewKeyword(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.NewKeyword,
                                                                        TriviaList(
                                                                            Space)))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            IdentifierName("item"),
                                                                                            IdentifierName("Key")))))),
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapToIDictionary}\n");
    }
    internal static void FromIDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapFromIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("IDictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapFromIDictionary"))
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
                        SingletonSeparatedList<ParameterSyntax>(
                            Parameter(
                                    Identifier("source"))
                                .WithType(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("IDictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
                                                                .WithNewKeyword(
                                                                    Token(
                                                                        TriviaList(),
                                                                        SyntaxKind.NewKeyword,
                                                                        TriviaList(
                                                                            Space)))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                    SingletonSeparatedList<
                                                                                        ArgumentSyntax>(
                                                                                        Argument(
                                                                                            MemberAccessExpression(
                                                                                                SyntaxKind
                                                                                                    .SimpleMemberAccessExpression,
                                                                                                IdentifierName("item"),
                                                                                                IdentifierName(
                                                                                                    "Key")))))),
                                                                        IdentifierName("MapFrom")))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList<ArgumentSyntax>(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("item"),
                                                                                    IdentifierName("Value")))))))
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapFromIDictionary}\n");
    }
    internal static void ToIReadOnlyDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapToIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("IReadOnlyDictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapToIReadOnlyDictionary"))
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
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("IReadOnlyDictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
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
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                        MemberAccessExpression(
                                                                                            SyntaxKind
                                                                                                .SimpleMemberAccessExpression,
                                                                                            IdentifierName("item"),
                                                                                            IdentifierName("Key")))))),
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapToIDictionary}\n");
    }
    internal static void FromIReadOnlyDictionaryMethod(StringBuilder classBuilder, string target, string source)
    {
        var mapFromIDictionary = LocalFunctionStatement(
                QualifiedName(
                    QualifiedName(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("Collections")),
                        IdentifierName("Generic")),
                    GenericName(
                            Identifier("IReadOnlyDictionary"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                    SeparatedList<TypeSyntax>(
                                        new SyntaxNodeOrToken[]
                                        {
                                            PredefinedType(
                                                Token(SyntaxKind.IntKeyword)),
                                            Token(
                                                TriviaList(),
                                                SyntaxKind.CommaToken,
                                                TriviaList(
                                                    Space)),
                                            IdentifierName(target)
                                        }))
                                .WithGreaterThanToken(
                                    Token(
                                        TriviaList(),
                                        SyntaxKind.GreaterThanToken,
                                        TriviaList(
                                            Space))))),
                Identifier("MapFromIReadOnlyDictionary"))
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
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Collections")),
                                            IdentifierName("Generic")),
                                        GenericName(
                                                Identifier("IReadOnlyDictionary"))
                                            .WithTypeArgumentList(
                                                TypeArgumentList(
                                                        SeparatedList<TypeSyntax>(
                                                            new SyntaxNodeOrToken[]
                                                            {
                                                                PredefinedType(
                                                                    Token(SyntaxKind.IntKeyword)),
                                                                Token(
                                                                    TriviaList(),
                                                                    SyntaxKind.CommaToken,
                                                                    TriviaList(
                                                                        Space)),
                                                                IdentifierName(source)
                                                            }))
                                                    .WithGreaterThanToken(
                                                        Token(
                                                            TriviaList(),
                                                            SyntaxKind.GreaterThanToken,
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
                                                            ObjectCreationExpression(
                                                                    QualifiedName(
                                                                        QualifiedName(
                                                                            QualifiedName(
                                                                                IdentifierName("System"),
                                                                                IdentifierName("Collections")),
                                                                            IdentifierName("Generic")),
                                                                        GenericName(
                                                                                Identifier("Dictionary"))
                                                                            .WithTypeArgumentList(
                                                                                TypeArgumentList(
                                                                                    SeparatedList<TypeSyntax>(
                                                                                        new SyntaxNodeOrToken[]
                                                                                        {
                                                                                            PredefinedType(
                                                                                                Token(SyntaxKind
                                                                                                    .IntKeyword)),
                                                                                            Token(
                                                                                                TriviaList(),
                                                                                                SyntaxKind.CommaToken,
                                                                                                TriviaList(
                                                                                                    Space)),
                                                                                            NullableType(
                                                                                                IdentifierName(
                                                                                                    target))
                                                                                        })))))
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
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("source"),
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
                        ForEachStatement(
                                IdentifierName(
                                    Identifier(
                                        TriviaList(),
                                        SyntaxKind.VarKeyword,
                                        "var",
                                        "var",
                                        TriviaList(
                                            Space))),
                                Identifier(
                                    TriviaList(),
                                    "item",
                                    TriviaList(
                                        Space)),
                                IdentifierName("source"),
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
                                                                                    MemberAccessExpression(
                                                                                        SyntaxKind
                                                                                            .SimpleMemberAccessExpression,
                                                                                        IdentifierName("item"),
                                                                                        IdentifierName("Key")))))
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
                                                                                            MemberAccessExpression(
                                                                                                SyntaxKind
                                                                                                    .SimpleMemberAccessExpression,
                                                                                                IdentifierName("item"),
                                                                                                IdentifierName(
                                                                                                    "Key")))))),
                                                                        IdentifierName("MapFrom")))
                                                                .WithArgumentList(
                                                                    ArgumentList(
                                                                        SingletonSeparatedList(
                                                                            Argument(
                                                                                MemberAccessExpression(
                                                                                    SyntaxKind
                                                                                        .SimpleMemberAccessExpression,
                                                                                    IdentifierName("item"),
                                                                                    IdentifierName("Value")))))))
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
                            .WithForEachKeyword(
                                Token(
                                    TriviaList(
                                        Whitespace("        ")),
                                    SyntaxKind.ForEachKeyword,
                                    TriviaList(
                                        Space)))
                            .WithInKeyword(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.InKeyword,
                                    TriviaList(
                                        Space)))
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
        classBuilder.Append($"\t{mapFromIDictionary}\n");
    }
    internal static void FromStack(StringBuilder classBuilder, string target, string source)
    {
        classBuilder.AppendLine(
            $"\n");
        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.Stack<{target}> MapFromStack(System.Collections.Generic.Stack<{source}> source)");
        classBuilder.AppendLine(
            $"\t{{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.Stack<{target}>();");
        classBuilder.AppendLine(
            $"\t\treturn target;");
        classBuilder.AppendLine(
            $"\t}}");
    }
    internal static void ToStack(StringBuilder classBuilder, string target, string source)
    {
        classBuilder.AppendLine(
            $"\n");
        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.Stack<{target}> MapToStack(System.Collections.Generic.Stack<{source}> source)");
        classBuilder.AppendLine(
            $"\t{{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.Stack<{target}>();");
        classBuilder.AppendLine(
            $"\t\treturn target;");
        classBuilder.AppendLine(
            $"\t}}");
    }
    internal static void FromQueue(StringBuilder classBuilder, string target, string source)
    {
        classBuilder.AppendLine(
            $"\n");
        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.Queue<{target}> MapFromQueue(System.Collections.Generic.Queue<{source}> source)");
        classBuilder.AppendLine(
            $"\t{{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.Queue<{target}>();");
        classBuilder.AppendLine(
            $"\t\treturn target;");
        classBuilder.AppendLine(
            $"\t}}");
    }
    internal static void ToQueue(StringBuilder classBuilder, string target, string source)
    {
        classBuilder.AppendLine(
            $"\n");
        classBuilder.AppendLine(
            $"\tinternal static System.Collections.Generic.Queue<{target}> MapToQueue(System.Collections.Generic.Queue<{source}> source)");
        classBuilder.AppendLine(
            $"\t{{");
        classBuilder.AppendLine(
            $"\t\tvar target = new System.Collections.Generic.Queue<{target}>();");
        classBuilder.AppendLine(
            $"\t\treturn target;");
        classBuilder.AppendLine(
            $"\t}}");
    }
}