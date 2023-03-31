using System;

namespace DynatestSourceGenerator.Abstractions.Attributes
{
    /// <summary>
    /// Attribute used to map an existing DTO property to a corresponding property of the original class. When applied to a property, it indicates that the generated DTO should use an existing DTO property instead of creating a new one.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class UseExistingDto : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UseExistingDto"/> class.
        /// </summary>
        /// <param name="classNames">The names of the DTO classes that the property should be mapped from.</param>
        public UseExistingDto(params string[] classNames)
        {
            ClassNames = classNames;
        }

        /// <summary>
        /// Gets or sets the names of the DTO classes that the property should be mapped from.
        /// </summary>
        public string[] ClassNames { get; set; }
    }
}
