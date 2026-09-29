using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FurnitureRepairConfig))]
public sealed class FurnitureRepairConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxCost"), new GUIContent("Макс. стоимость починки"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_breakSeconds"), new GUIContent("Время до полной поломки, секунды"));
        serializedObject.ApplyModifiedProperties();
    }
}
