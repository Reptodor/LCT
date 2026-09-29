using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
static class WardrobeButtonPlacer
{
    static readonly Color Wardrobe = new Color(0.55f, 0.36f, 0.20f, 1f);

    static WardrobeButtonPlacer()
    {
        EditorApplication.delayCall += PlaceIfMissing;
    }

    static void PlaceIfMissing()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += PlaceIfMissing;
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        var snack = GameObject.Find("SnackButton");
        if (snack == null)
        {
            return;
        }

        var bar = snack.transform.parent;
        if (bar == null || bar.Find("WardrobeButton") != null)
        {
            return;
        }

        var copy = Object.Instantiate(snack, bar);
        copy.name = "WardrobeButton";
        var label = copy.GetComponentInChildren<TMP_Text>();
        if (label != null)
        {
            label.text = "Расстановка";
        }

        var image = copy.GetComponent<Image>();
        if (image != null)
        {
            image.color = Wardrobe;
        }

        var button = copy.GetComponent<Button>();
        if (button != null)
        {
            button.onClick = new Button.ButtonClickedEvent();
        }

        EditorSceneManager.MarkSceneDirty(copy.scene);
        EditorSceneManager.SaveScene(copy.scene);
        Debug.Log("[LCT] Wardrobe button placed in the scene");
    }
}
