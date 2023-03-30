using DynatestSourceGenerator.DataTransferObject.Attributes;

namespace DynatestSourceGenerator.Tests.Attributes;

[TestFixture]
public class GenerateDtoTests
{
    [Test]
    public void CanCreateGenerateDTO()
    {
        // Arrange
        var classNames = new[] { "Class1", "Class2" };

        // Act
        var generateDto = new GenerateDto(classNames);

        // Assert
        Assert.AreEqual(classNames, generateDto.ClassNames);
    }

}