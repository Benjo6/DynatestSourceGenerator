using System;

namespace DynatestSourceGenerator.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class GenerateDto : Attribute
{
    public GenerateDto(params string[] classNames)
    {
        ClassNames = classNames;
    }
        
    public GenerateDto(bool useDynamic, params string[] classNames)
    {
        UseDynamic = useDynamic;
        ClassNames = classNames;
    }

    public bool UseDynamic { get; set; }
    public string[] ClassNames { get; set; }
}