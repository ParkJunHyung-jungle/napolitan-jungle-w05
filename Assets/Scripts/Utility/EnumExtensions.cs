using System;
using System.Collections.Generic;

public static class EnumExtensions
{
    public static T Next<T>(this T value) where T : struct, Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        int i = Array.IndexOf(values, value);
        return values[(i + 1) % values.Length];
    }
}