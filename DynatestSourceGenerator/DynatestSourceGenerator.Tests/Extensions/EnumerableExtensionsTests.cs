using DynatestSourceGenerator.DataTransferObject.Extensions;
using FluentAssertions;

namespace DynatestSourceGenerator.Tests.Extensions;

[TestFixture]
public class EnumerableExtensionsTests
{
    [Test]
    public void WhereNotNullWhenValueTypeShouldFilterNulls()
    {
        new int?[] { 1, null, null, 2, 3, 4, null }
            .WhereNotNull()
            .Should()
            .BeEquivalentTo(new[] { 1, 2, 3, 4 }, o => o.WithStrictOrdering());
    }

    [Test]
    public void WhereNotNullShouldFilterNulls()
    {
        new[] { "a", "b", "c", null, "d", null, null, "e" }
            .WhereNotNull()
            .Should()
            .BeEquivalentTo(new[] { "a", "b", "c", "d", "e" }, o => o.WithStrictOrdering());
    }
}