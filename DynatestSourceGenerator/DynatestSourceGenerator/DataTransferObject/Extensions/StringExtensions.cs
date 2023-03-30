﻿using System;

namespace DynatestSourceGenerator.DataTransferObject.Extensions;

public static class StringExtensions
{
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search, StringComparison.Ordinal);
        if (pos < 0)
        {
            return text;
        }
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }

    public static string GetLastPart(this string text, string split)
    {
        var lastIndex = text.LastIndexOf(split, StringComparison.Ordinal);
        if (lastIndex + split.Length < text.Length)
        {
            return text.Substring(lastIndex + split.Length);
        }

        return null;
    }
}