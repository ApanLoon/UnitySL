using System;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public class UIntEnumAttribute : PropertyAttribute
    {
        public Type EnumType;
        public bool Flags;

        public UIntEnumAttribute(Type type, bool flags = false)
        {
            EnumType = type;
            Flags = flags;
        }
    }
}
