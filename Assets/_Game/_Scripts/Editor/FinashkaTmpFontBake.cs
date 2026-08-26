using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class FinashkaTmpFontBake
{
    const string FallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
    const string MainPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    const string SettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
    const string SessionKey = "Finashka.TmpCyrillic.v1";
    const uint Ya = 0x042F;

    static FinashkaTmpFontBake()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Finashka/Bake Cyrillic Font")]
    static void BakeMenu()
    {
        Bake(true);
    }

    static void RunOnce()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        SessionState.SetBool(SessionKey, true);
        Bake(false);
    }

    static void Bake(bool force)
    {
        var fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FallbackPath);
        if (fallback == null)
        {
            return;
        }

        fallback.ReadFontAssetDefinition();
        bool hasCyrillic = fallback.characterLookupTable != null && fallback.characterLookupTable.ContainsKey(Ya);
        if (!force && hasCyrillic && fallback.atlasPopulationMode == AtlasPopulationMode.Static)
        {
            Wire(fallback);
            return;
        }

        fallback.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        fallback.isMultiAtlasTexturesEnabled = true;
        string missing;
        fallback.TryAddCharacters(Charset(), out missing);
        fallback.atlasPopulationMode = AtlasPopulationMode.Static;
        EditorUtility.SetDirty(fallback);
        Wire(fallback);
        AssetDatabase.SaveAssets();
        if (!string.IsNullOrEmpty(missing))
        {
            Debug.LogWarning("Finashka font bake missing glyphs: " + missing.Length);
        }
    }

    static void Wire(TMP_FontAsset fallback)
    {
        AddToFontList(MainPath, fallback);
        var settings = AssetDatabase.LoadAssetAtPath<Object>(SettingsPath);
        if (settings == null)
        {
            return;
        }

        var so = new SerializedObject(settings);
        SerializedProperty features = so.FindProperty("m_GetFontFeaturesAtRuntime");
        if (features != null)
        {
            features.boolValue = false;
        }

        AddObjectToList(so, "m_fallbackFontAssets", fallback);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(settings);
    }

    static void AddToFontList(string path, TMP_FontAsset fallback)
    {
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (font == null)
        {
            return;
        }

        var so = new SerializedObject(font);
        if (!AddObjectToList(so, "m_FallbackFontAssetTable", fallback))
        {
            AddObjectToList(so, "fallbackFontAssets", fallback);
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(font);
    }

    static bool AddObjectToList(SerializedObject so, string property, Object value)
    {
        SerializedProperty list = so.FindProperty(property);
        if (list == null || !list.isArray)
        {
            return false;
        }

        for (int i = 0; i < list.arraySize; i++)
        {
            if (list.GetArrayElementAtIndex(i).objectReferenceValue == value)
            {
                return true;
            }
        }

        list.arraySize += 1;
        list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = value;
        return true;
    }

    static string Charset()
    {
        var sb = new StringBuilder(512);
        for (int i = 32; i <= 126; i++)
        {
            sb.Append((char)i);
        }

        for (int i = 160; i <= 255; i++)
        {
            sb.Append((char)i);
        }

        for (int i = 0x0400; i <= 0x04FF; i++)
        {
            sb.Append((char)i);
        }

        sb.Append('\u2026');
        sb.Append('\u2013');
        sb.Append('\u2014');
        sb.Append('\u2116');
        return sb.ToString();
    }
}
