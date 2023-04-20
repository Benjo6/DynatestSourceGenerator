using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

public static class Types
{
    public static TypeSyntax IDictionaryT => ParseTypeName(typeof(IDictionary<,>).FullName);
    public static TypeSyntax IReadOnlyDictionaryT => ParseTypeName(typeof(IReadOnlyDictionary<,>).FullName);
    public static TypeSyntax IEnumerableT =>  ParseTypeName(typeof(IEnumerable<>).FullName);
    public static TypeSyntax Enumerable => ParseTypeName(typeof(Enumerable).FullName);
    public static TypeSyntax ICollection => ParseTypeName(typeof(System.Collections.ICollection).FullName);
    public static TypeSyntax ICollectionT =>  ParseTypeName(typeof(ICollection<>).FullName);
    public static TypeSyntax IReadOnlyCollectionT => ParseTypeName(typeof(IReadOnlyCollection<>).FullName);
    public static TypeSyntax IListT => ParseTypeName(typeof(IList<>).FullName);
    public static TypeSyntax ListT =>  ParseTypeName(typeof(List<>).FullName);
    public static TypeSyntax StackT =>  ParseTypeName(typeof(Stack<>).FullName);
    public static TypeSyntax QueueT => ParseTypeName(typeof(Queue<>).FullName);
    public static TypeSyntax IReadOnlyListT =>  ParseTypeName(typeof(IReadOnlyList<>).FullName);
    public static TypeSyntax KeyValuePairT => ParseTypeName(typeof(KeyValuePair<,>).FullName);
    public static TypeSyntax DictionaryT =>  ParseTypeName(typeof(Dictionary<,>).FullName);
    public static TypeSyntax Enum => ParseTypeName(typeof(Enum).FullName);
    public static TypeSyntax IQueryableT =>  ParseTypeName(typeof(IQueryable<>).FullName);
    public static TypeSyntax ImmutableArray => ParseTypeName(typeof(ImmutableArray).FullName);
    public static TypeSyntax ImmutableArrayT => ParseTypeName(typeof(ImmutableArray<>).FullName);
    public static TypeSyntax ImmutableList => ParseTypeName(typeof(ImmutableList).FullName);
    public static TypeSyntax ImmutableListT => ParseTypeName(typeof(ImmutableList<>).FullName);
    public static TypeSyntax ImmutableHashSet => ParseTypeName(typeof(ImmutableHashSet).FullName);
    public static TypeSyntax ImmutableHashSetT => ParseTypeName(typeof(ImmutableHashSet<>).FullName);
    public static TypeSyntax ImmutableQueue =>  ParseTypeName(typeof(ImmutableQueue).FullName);
    public static TypeSyntax ImmutableQueueT => ParseTypeName(typeof(ImmutableQueue<>).FullName);
    public static TypeSyntax ImmutableStack =>  ParseTypeName(typeof(ImmutableStack).FullName);
    public static TypeSyntax ImmutableStackT => ParseTypeName(typeof(ImmutableStack<>).FullName);
    public static TypeSyntax ImmutableSortedSet =>  ParseTypeName(typeof(ImmutableSortedSet).FullName);
    public static TypeSyntax ImmutableSortedSetT =>  ParseTypeName(typeof(ImmutableSortedSet<>).FullName);
    public static TypeSyntax ImmutableDictionary => ParseTypeName(typeof(ImmutableDictionary).FullName);
    public static TypeSyntax IImmutableDictionaryT => ParseTypeName(typeof(IImmutableDictionary<,>).FullName);
    public static TypeSyntax ImmutableDictionaryT =>  ParseTypeName(typeof(ImmutableDictionary<,>).FullName);
    public static TypeSyntax ImmutableSortedDictionary => ParseTypeName(typeof(ImmutableSortedDictionary).FullName);
    public static TypeSyntax ImmutableSortedDictionaryT => ParseTypeName(typeof(ImmutableSortedDictionary<,>).FullName);
    // use string type name as they are not available in netstandard2.0
    public static TypeSyntax? DateOnly => ParseTypeName("System.DateOnly");
    public static TypeSyntax? TimeOnly => ParseTypeName("System.TimeOnly");
    
    private static TypeSyntax ParseTypeName(string typeName)
    {
        return SyntaxFactory.ParseTypeName(typeName);
    }
}