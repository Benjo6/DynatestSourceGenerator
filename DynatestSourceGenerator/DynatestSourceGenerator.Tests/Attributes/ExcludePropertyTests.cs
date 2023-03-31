using DynatestSourceGenerator.Abstractions.Attributes;

namespace DynatestSourceGenerator.Tests.Attributes;

[TestFixture]
public class ExcludePropertyTests
{
    [Test]
    public void CanCreateExcludeProperty()
    {
        // Arrange
        var classNames = new[] { "Class1", "Class2" };

        // Act
        var excludeProperty = new ExcludeProperty(classNames);

        // Assert
        Assert.AreEqual(classNames, excludeProperty.ClassNames);
    }
}