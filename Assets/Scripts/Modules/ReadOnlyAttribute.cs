using UnityEngine;
using UnityEditor;

public sealed class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public sealed class ReadOnlyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        using (new EditorGUI.DisabledScope(true))
            EditorGUI.PropertyField(pos, prop, label, true);
    }
}
#endif