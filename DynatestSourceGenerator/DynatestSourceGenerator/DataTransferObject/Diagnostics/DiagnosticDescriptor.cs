using Microsoft.CodeAnalysis;

namespace DynatestSourceGenerator.DataTransferObject.Diagnostics;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedMappingMethodSignature = new DiagnosticDescriptor(
        "DSG001",
        "Has an unsupported mapping method signature",
        "{0} has an unsupported mapping method signature",
        DiagnosticCategories.Mapper,
        DiagnosticSeverity.Error,
        true
    );
}
