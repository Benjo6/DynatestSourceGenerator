namespace DynatestSourceGenerator.Enums;

public enum DataStructureType
{
    /// <summary>Generate a standard C# DTO class</summary>
    Class,
    /// <summary>Generate a DTO record class</summary>
    RecordClass,
    /// <summary>Generate a standard C# DTO struct</summary>
    Struct,
    /// <summary>Generate a DTO record struct</summary>
    RecordStruct
}