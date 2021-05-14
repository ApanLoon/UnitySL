using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Extensions.Editor
{

    [CustomPropertyDrawer(typeof(UIntEnumAttribute))]
    public class UIntEnumDrawer : PropertyDrawer
    {
        public UIntEnumAttribute EnumAttribute => (UIntEnumAttribute)attribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Enum value = (Enum)Enum.ToObject(EnumAttribute.EnumType, (uint)property.longValue);
            value = EnumAttribute.Flags ? EditorGUI.EnumFlagsField(position, label, value) : EditorGUI.EnumPopup(position, label, value);

            property.longValue = (uint)Enum.ToObject(EnumAttribute.EnumType, value);
        }
    }
}
