using System;

namespace DynatestSourceGenerator.Abstractions.Attributes
{
    /// <summary>
    /// This attribute is used to mark a class as a source for DTO generation. 
    /// When applied to a class, it will trigger the generator to create a corresponding DTO
    /// with the same properties as the original class, but with a "DTO" suffix.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class GenerateDto : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the GenerateDto class with the specified class names.
        /// </summary>
        /// <param name="classNames">The names of the classes to generate DTOs for.</param>
        public GenerateDto(params string[] classNames)
        {
            ClassNames = classNames;
        }
        /// <summary>
        /// Gets or sets the names of the classes to generate DTOs for.
        /// </summary>
        public string[] ClassNames { get; set; }
    }
}
