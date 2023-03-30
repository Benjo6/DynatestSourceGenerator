using System;

namespace DynatestSourceGenerator.DataTransferObject.Extensions;

public static class StringExtensions
{
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.Ordinal);
        return pos < 0 ? text : text[..pos] + replace + text[(pos + search.Length)..];
    }

    public static string GetLastPart(this string text, string split)
    {
        var lastIndex = text.LastIndexOf(split, StringComparison.Ordinal);
        return lastIndex + split.Length < text.Length ? text[(lastIndex + split.Length)..] : null;
    }
}