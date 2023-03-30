using DynatestSourceGenerator.DataTransferObject.Attributes;

namespace DynatestSourceGenerator.Tests.Attributes;

[TestFixture]
public class UseExistingDtoTests
{
    [Test]
    public void CanCreateUseExistingDto()
    {
        // Arrange
        var classNames = new[] { "Class1", "Class2" };

        // Act
        var useExistingDto = new UseExistingDto(classNames);

        // Assert
        Assert.AreEqual(classNames, useExistingDto.ClassNames);
    }
}