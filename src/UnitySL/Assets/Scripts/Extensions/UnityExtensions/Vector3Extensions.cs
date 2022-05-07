using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.Extensions.UnityExtensions
{
    public static class Vector3Extensions
    {
        public static void Parse(this Vector3 v, string s)
        {
            string[] components = Regex.Split(s.Trim(), "\\s?,?\\s+");
            if (components.Length != 3)
            {
                throw new ArgumentException($"A vector3 requires three components separated by a comma or space. Got \"{s}\"");
            }

            v.x = float.Parse(components[0], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            v.y = float.Parse(components[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); // Handedness
            v.z = float.Parse(components[1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat); // Handedness
        }
    }
}
