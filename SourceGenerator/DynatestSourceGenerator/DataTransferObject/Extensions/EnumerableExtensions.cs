using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DynatestSourceGenerator.DataTransferObject.Extensions;

public static class EnumerableExtensions
{

    public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T> valuesProvider)
    {
#nullable disable
        return valuesProvider.Where(x => x != null);
#nullable restore
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable)
        where T : struct
    {
#nullable disable
        return enumerable.Where(x => x != null).Select(x => x.Value);
#nullable restore
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable)
    {
#nullable disable
        return enumerable.Where(x => x != null);
#nullable restore
    }
}