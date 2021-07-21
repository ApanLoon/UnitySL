using System;
using System.Collections.Generic;

namespace Assets.Scripts.Extensions.SystemExtensions
{
    public static class ListExtensions
    {
        public static bool IsEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }

        public static T FirstItem<T>(this List<T> list) where T : class
        {
            return list.Count == 0 ? null : list[0];
        }

        public static T LastItem<T>(this List<T> list) where T : class
        {
            return list.Count == 0 ? null : list[list.Count - 1];
        }

    }
}
