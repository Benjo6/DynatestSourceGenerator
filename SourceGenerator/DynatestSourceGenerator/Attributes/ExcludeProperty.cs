using System;

namespace DynatestSourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ExcludeProperty : Attribute
{
    public ExcludeProperty(params string[] classNames)
    {
        ClassNames = classNames;
    }

    public string[] ClassNames { get; set; }
}