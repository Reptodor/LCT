using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class FinashkaAndroidGameView
{
    const int Width = 1080;
    const int Height = 1920;
    const string SizeName = "Finashka Android";

    static FinashkaAndroidGameView()
    {
        EditorApplication.delayCall += Apply;
    }

    [MenuItem("Finashka/Android Game View (1080x1920)")]
    public static void Apply()
    {
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type sizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        Type sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
        Type sizeKindType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
        if (sizesType == null || gameViewType == null || sizeType == null || sizeKindType == null)
        {
            Debug.LogWarning("Finashka: не удалось открыть Game View API.");
            return;
        }

        object instance = sizesType.BaseType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
        object group = sizesType.GetProperty("currentGroup").GetValue(instance, null);
        Type groupType = group.GetType();
        MethodInfo getSize = groupType.GetMethod("GetGameViewSize", flags);
        int count = (int)groupType.GetMethod("GetTotalCount", flags).Invoke(group, null);
        int found = -1;
        for (int i = 0; i < count; i++)
        {
            object size = getSize.Invoke(group, new object[] { i });
            int w = (int)size.GetType().GetProperty("width").GetValue(size, null);
            int h = (int)size.GetType().GetProperty("height").GetValue(size, null);
            if (w == Width && h == Height)
            {
                found = i;
                break;
            }
        }

        if (found < 0)
        {
            object fixedRes = Enum.Parse(sizeKindType, "FixedResolution");
            object custom = Activator.CreateInstance(sizeType, new object[] { fixedRes, Width, Height, SizeName });
            groupType.GetMethod("AddCustomSize", flags).Invoke(group, new object[] { custom });
            found = (int)groupType.GetMethod("GetTotalCount", flags).Invoke(group, null) - 1;
        }

        EditorWindow gameView = EditorWindow.GetWindow(gameViewType, false, null, true);
        gameViewType.GetMethod("SizeSelectionCallback", flags).Invoke(gameView, new object[] { found, null });
        gameView.Repaint();
        Debug.Log("Finashka: Game View = " + Width + "x" + Height + " (Android portrait).");
    }
}
