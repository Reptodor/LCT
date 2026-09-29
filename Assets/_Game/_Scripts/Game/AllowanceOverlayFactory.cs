using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class AllowanceOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.11f, 0.26f, 0.15f, 0.98f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);

    public static void Build(Transform canvas, AllowanceWindow view, TMP_FontAsset font)
    {
        Transform existing = canvas.Find("AllowanceOverlay");
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        var overlay = new GameObject("AllowanceOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster));
        Stretch(overlay, canvas);
        var layer = overlay.GetComponent<Canvas>();
        layer.overrideSorting = true;
        layer.sortingOrder = 200;
        var group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        overlay.SetActive(false);

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        Stretch(dimGo, overlay.transform);
        Paint(dimGo, Dim, true);

        var safe = new GameObject("SheetSafe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, overlay.transform);
        Column(safe, new RectOffset(48, 48, 48, 48), 0f, TextAnchor.MiddleCenter);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        Paint(sheet, Sheet, true);
        var sheetLayout = sheet.GetComponent<LayoutElement>();
        sheetLayout.preferredHeight = 520f;
        sheetLayout.flexibleHeight = 0f;
        sheetLayout.flexibleWidth = 1f;

        var motion = new GameObject("Motion", typeof(RectTransform));
        Stretch(motion, sheet.transform);
        Column(motion, new RectOffset(32, 32, 36, 32), 20f, TextAnchor.MiddleCenter);

        var title = Label(motion.transform, "Title", "Выплата", 32f, 48f, FontStyles.Bold, Gold, TextAlignmentOptions.Center, font);
        PrefHeight(title.gameObject, 72f);
        var message = Label(motion.transform, "Message", "Вы получили 0 монет", 26f, 40f, FontStyles.Bold, Cream, TextAlignmentOptions.Center, font);
        PrefHeight(message.gameObject, 120f);
        Button confirm = WideButton(motion.transform, "ConfirmButton", "Подтвердить", Gold, Ink, font);

        view.Setup(overlay, confirm, message, group, motion.GetComponent<RectTransform>());
    }

    static Button WideButton(Transform parent, string name, string text, Color fill, Color labelColor, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        Paint(go, fill, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 96f;
        layout.preferredHeight = 96f;
        layout.flexibleWidth = 1f;
        var button = go.GetComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        var label = Label(go.transform, "Label", text, 22f, 36f, FontStyles.Bold, labelColor, TextAlignmentOptions.Center, font);
        var rect = label.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return button;
    }

    static void Stretch(GameObject go, Transform parent)
    {
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void Paint(GameObject go, Color color, bool raycast)
    {
        var image = go.GetComponent<Image>();
        if (image == null)
        {
            image = go.AddComponent<Image>();
        }

        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = raycast;
    }

    static void Column(GameObject go, RectOffset padding, float spacing, TextAnchor align)
    {
        var layout = go.GetComponent<VerticalLayoutGroup>();
        if (layout == null)
        {
            layout = go.AddComponent<VerticalLayoutGroup>();
        }

        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = align;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    static void PrefHeight(GameObject go, float height)
    {
        var layout = go.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = go.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
        layout.flexibleHeight = 0f;
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = min;
        tmp.fontSizeMax = max;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        if (font != null)
        {
            tmp.font = font;
        }
        else if (TMP_Settings.defaultFontAsset != null)
        {
            tmp.font = TMP_Settings.defaultFontAsset;
        }

        return tmp;
    }
}
