using DynatestSourceGenerator.DataTransferObject.Extensions;

namespace DynatestSourceGenerator.Tests.Extensions;

[TestFixture]
public class StringExtensionsTests
{
    [Test]
    public void ReplaceFirstShouldReplace_FirstOccurrence()
    {
        // Arrange
        var input = "Hello World";
        
        // Act
        var result = input.ReplaceFirst("H", "B");
        
        // Assert
        Assert.That(result,Is.EqualTo("Bello World"));
    }

    [Test]
    public void ReplaceFirstWhenSearchStringNotFoundShould_NotReplace()
    { 
        // Arrange
        var input = "Hello World";
        
        // Act
        var result = input.ReplaceFirst("Z", "B");
        
        // Assert
        Assert.That(result,Is.EqualTo("Hello World"));    
    }

    [Test]
    public void GetLastPartShouldReturnTheLastPartOfTheText()
    {
        // Arrange
        var input = "C:\\Users\\Benjo\\Downloads\\file.txt";

        // Act
        var result = input.GetLastPart("\\");

        // Assert
        Assert.That("file.txt",Is.EqualTo(result));
    }

    [Test]
    public void GetLastPartWhenSplitStringNotFoundShouldReturnInput()
    {
        // Arrange
        var input = "Hello World\\Hello";
        
        // Act
        var result = input.GetLastPart(",");
        
        // Assert
        Assert.That(result,Is.EqualTo(input));
    }
}