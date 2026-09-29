using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FurnitureRepairOverlayFactory
{
    static readonly Color Cream = new Color(1f, 0.97f, 0.88f, 1f);
    static readonly Color Gold = new Color(0.96f, 0.78f, 0.28f, 1f);
    static readonly Color Dim = new Color(0.04f, 0.08f, 0.03f, 0.78f);
    static readonly Color Sheet = new Color(0.11f, 0.26f, 0.15f, 0.98f);
    static readonly Color Ink = new Color(0.14f, 0.16f, 0.08f, 1f);
    static readonly Color Track = new Color(0.08f, 0.14f, 0.08f, 1f);
    static Sprite _solid;

    public static void Build(Transform canvas, FurnitureRepairWindow view, TMP_FontAsset font)
    {
        Transform existing = canvas.Find("FurnitureRepairOverlay");
        if (existing != null)
        {
            Object.Destroy(existing.gameObject);
        }

        var overlay = new GameObject("FurnitureRepairOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster));
        Stretch(overlay, canvas);
        var layer = overlay.GetComponent<Canvas>();
        layer.overrideSorting = true;
        layer.sortingOrder = 180;
        var group = overlay.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
        overlay.SetActive(false);

        var dimGo = new GameObject("Dim", typeof(RectTransform), typeof(Image), typeof(Button));
        Stretch(dimGo, overlay.transform);
        Paint(dimGo, Dim, true);
        var dimButton = dimGo.GetComponent<Button>();
        dimButton.targetGraphic = dimGo.GetComponent<Image>();
        dimButton.transition = Selectable.Transition.None;

        var safe = new GameObject("SheetSafe", typeof(RectTransform), typeof(SafeAreaFitter));
        Stretch(safe, overlay.transform);
        Column(safe, new RectOffset(72, 72, 72, 72), 0f, TextAnchor.MiddleCenter);

        var sheet = new GameObject("Sheet", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        sheet.transform.SetParent(safe.transform, false);
        Paint(sheet, Sheet, true);
        var sheetLayout = sheet.GetComponent<LayoutElement>();
        sheetLayout.preferredHeight = 460f;
        sheetLayout.flexibleHeight = 0f;
        sheetLayout.flexibleWidth = 1f;

        var motion = new GameObject("Motion", typeof(RectTransform));
        Stretch(motion, sheet.transform);
        Column(motion, new RectOffset(32, 32, 28, 28), 18f, TextAnchor.MiddleCenter);

        var title = Label(motion.transform, "Title", "Мебель", 32f, 48f, FontStyles.Bold, Gold, TextAlignmentOptions.Center, font);
        PrefHeight(title.gameObject, 72f);

        Image fill = HealthBar(motion.transform);
        Button repair = WideButton(motion.transform, "RepairButton", "Починить", Gold, Ink, font);
        Button close = WideButton(motion.transform, "CloseButton", "Закрыть", new Color(0.45f, 0.28f, 0.16f, 1f), Cream, font);

        view.Setup(overlay, title, fill, repair, close, dimButton, group);
    }

    static Image HealthBar(Transform parent)
    {
        var trackGo = new GameObject("HealthTrack", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        trackGo.transform.SetParent(parent, false);
        var layout = trackGo.GetComponent<LayoutElement>();
        layout.minHeight = 36f;
        layout.preferredHeight = 36f;
        layout.flexibleWidth = 1f;
        var track = trackGo.GetComponent<Image>();
        track.sprite = Solid();
        track.color = Track;
        track.raycastTarget = false;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(trackGo.transform, false);
        var fillRect = fillGo.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(4f, 4f);
        fillRect.offsetMax = new Vector2(-4f, -4f);
        var fill = fillGo.GetComponent<Image>();
        fill.sprite = Solid();
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = (int)Image.OriginHorizontal.Left;
        fill.color = new Color(0.45f, 0.78f, 0.32f, 1f);
        fill.raycastTarget = false;
        fill.fillAmount = 1f;
        return fill;
    }

    static Button WideButton(Transform parent, string name, string text, Color fill, Color labelColor, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        Paint(go, fill, true);
        var layout = go.GetComponent<LayoutElement>();
        layout.minHeight = 88f;
        layout.preferredHeight = 88f;
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

    static Sprite Solid()
    {
        if (_solid == null)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            _solid = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }

        return _solid;
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
        image.sprite = Solid();
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
    }

    static TextMeshProUGUI Label(Transform parent, string name, string text, float min, float max, FontStyles style, Color color, TextAlignmentOptions align, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.font = font;
        label.fontStyle = style;
        label.alignment = align;
        label.enableAutoSizing = true;
        label.fontSizeMin = min;
        label.fontSizeMax = max;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }
}
