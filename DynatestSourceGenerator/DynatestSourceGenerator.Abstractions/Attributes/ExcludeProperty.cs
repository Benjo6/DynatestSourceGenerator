using System;

namespace DynatestSourceGenerator.Abstractions.Attributes
{
    /// <summary>
    /// This attribute is used to exclude a property from being included in the generated DTO. When applied to a property, it will be ignored in the DTO generation process.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ExcludeProperty : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExcludeProperty"/> class.
        /// </summary>
        /// <param name="classNames">An optional list of class names that this attribute applies to.</param>
        public ExcludeProperty(params string[] classNames)
        {
            ClassNames = classNames;
        }

        /// <summary>
        /// Gets or sets the list of class names that this attribute applies to.
        /// </summary>
        public string[] ClassNames { get; set; }
    }
}
