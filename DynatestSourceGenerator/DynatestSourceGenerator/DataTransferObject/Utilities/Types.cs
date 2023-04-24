using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DynatestSourceGenerator.DataTransferObject.Utilities;

public static class Types
{
    public const string IDictionaryT = "IDictionary";
    public const string IReadOnlyDictionaryT = "IReadOnlyDictionary";
    public const string IEnumerableT =  "IEnumerable";
    public const string Enumerable = "Enumerable";
    public const string ICollectionT = "ICollection";
    public const string IReadOnlyCollectionT = "IReadOnlyCollection";
    public const string IListT = "IList";
    public const string ListT =  "List";
    public const string StackT =  "Stack";
    public const string QueueT = "Queue";
    public const string IReadOnlyListT =  "IReadOnlyList";
    public const string KeyValuePairT = "KeyValuePair";
    public const string DictionaryT =  "Dictionary";
  //  public static TypeSyntax Enum => ParseTypeName(typeof(Enum).FullName);
    public const string IQueryableT =  "IQueryable";
    public const string ImmutableArrayT = "ImmutableArray";
    public const string ImmutableListT = "ImmutableList";
    public const string IImmutableListT = "IImmutableList";
    public const string ImmutableHashSetT = "ImmutableHashSet";
    public const string IImmutableSetT = "IImmutableSet";
    public const string ImmutableQueueT = "ImmutableQueue";
    public const string IImmutableQueueT = "IImmutableQueue";
    public const string ImmutableStackT = "ImmutableStack";
    public const string IImmutableStackT = "IImmutableStack";
    public const string ImmutableSortedSetT =  "ImmutableSortedSet";
    public const string IImmutableDictionaryT = "IImmutableDictionary";
    public const string ImmutableDictionaryT =  "ImmutableDictionary";
    public const string ImmutableSortedDictionaryT = "ImmutableSortedDictionary";
    // use string type name as they are not available in netstandard2.0
    public const string DateOnly = "System.DateOnly";
    public const string TimeOnly ="System.TimeOnly";
}