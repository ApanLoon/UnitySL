
using UnityEngine;

public static class ColorExtensions
{
    public static string ToRtfString(this Color color)
    {
        byte r = (byte)(color.r * 255);
        byte g = (byte)(color.g * 255);
        byte b = (byte)(color.b * 255);
        byte a = (byte)(color.a * 255);
        return $"<color=#{r:x2}{g:x2}{b:x2}{a:x2}>";
    }
}

