using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FurnitureWearConfig))]
public sealed class FurnitureWearConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxHp"), new GUIContent("Количество хп"));
        serializedObject.ApplyModifiedProperties();
    }
}
