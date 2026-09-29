using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FinashkaPetCustomizeBuilder
{
    const string ScenePath = "Assets/_Game/_Scenes/PetCustomize.unity";
    const string CatalogPath = "Assets/_Game/Resources/PetLookCatalog.asset";
    const string CatPath = "Assets/_Game/_Art/Catv3/Cat.prefab";

    [MenuItem("Finashka/Wire Pet Customize")]
    public static void Wire()
    {
        EnsureCatalog();
        EnsureBuildSettings();
        AssetDatabase.SaveAssets();
    }

    [InitializeOnLoadMethod]
    static void AutoWire()
    {
        EditorApplication.delayCall += DelayWire;
    }

    static void DelayWire()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += DelayWire;
            return;
        }

        if (!File.Exists(ScenePath))
        {
            return;
        }

        EnsureCatalog();
        EnsureBuildSettings();
    }

    static void EnsureCatalog()
    {
        GameObject cat = AssetDatabase.LoadAssetAtPath<GameObject>(CatPath);
        var catalog = AssetDatabase.LoadAssetAtPath<PetLookCatalog>(CatalogPath);
        if (catalog == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(CatalogPath) != null)
            {
                AssetDatabase.DeleteAsset(CatalogPath);
            }

            catalog = ScriptableObject.CreateInstance<PetLookCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        var so = new SerializedObject(catalog);
        bool dirty = Assign(so.FindProperty("_cat"), cat);
        if (!dirty)
        {
            return;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
    }

    static bool Assign(SerializedProperty property, Object value)
    {
        if (property == null || property.objectReferenceValue == value)
        {
            return false;
        }

        property.objectReferenceValue = value;
        return true;
    }

    static void EnsureBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path == ScenePath)
            {
                if (!scenes[i].enabled)
                {
                    scenes[i].enabled = true;
                    EditorBuildSettings.scenes = scenes;
                }

                return;
            }
        }

        var list = new List<EditorBuildSettingsScene>(scenes);
        list.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();
    }
}
