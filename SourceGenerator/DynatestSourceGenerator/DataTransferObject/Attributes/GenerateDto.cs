using System;
using DynatestSourceGenerator.DataTransferObject.Enums;

namespace DynatestSourceGenerator.DataTransferObject.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public class GenerateDto : Attribute
{
    public GenerateDto(params string[] classNames)
    {
        ClassNames = classNames;
    }

    public string[] ClassNames { get; set; }
}