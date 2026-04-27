using VarelaAloisio.Core.Attributes;
using UnityEditor;
using UnityEngine;

namespace VarelaAloisio.Core.Editor.Drawers
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool isReadOnly = attribute is ReadOnlyAttribute;

            EditorGUI.BeginDisabledGroup(isReadOnly);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }
}
