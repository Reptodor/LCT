using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HungerConfig))]
public sealed class HungerConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_intervalSeconds"), new GUIContent("Интервал, секунды"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_amount"), new GUIContent("Сколько голода забирает"));
        serializedObject.ApplyModifiedProperties();
    }
}
