namespace DynatestSourceGenerator.Extensions;

public static class StringExtensionMethods
{
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search);
        return pos < 0 ? text : text[..pos] + replace + text[(pos + search.Length)..];
    }

    public static string GetLastPart(this string text, string split)
    {
        var lastIndex = text.LastIndexOf(split);
        return lastIndex + split.Length < text.Length ? text[(lastIndex + split.Length)..] : null;
    }
}