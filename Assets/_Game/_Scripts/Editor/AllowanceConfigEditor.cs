using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AllowanceConfig))]
public sealed class AllowanceConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_amount"), new GUIContent("Сумма выплаты"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_intervalSeconds"), new GUIContent("Интервал, секунды"));
        serializedObject.ApplyModifiedProperties();
    }
}
